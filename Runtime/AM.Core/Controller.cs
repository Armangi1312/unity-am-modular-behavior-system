using AM.Core.Pipelines;
using AM.Core.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace AM.Core
{
    public abstract class Controller : MonoBehaviour
    {
#if UNITY_EDITOR
        public abstract Type SettingType();
        public abstract Type ContextType();
        public abstract Type ProcessorType();

        public abstract object GetSetting();
        public abstract object GetContext();
        public abstract object GetProcessor();
#endif
    }

    public abstract class Controller<TSetting, TContext, TProcessor> : Controller
        where TSetting : class, ISetting
        where TContext : class, IContext
        where TProcessor : class, IProcessor<TSetting, TContext>
    {
        [SerializeReference] protected Registry<TSetting> settings = new();
        [SerializeReference] protected Registry<TContext> contexts = new();
        [SerializeReference] protected List<TProcessor> processors = new();
        [SerializeReference] protected IPipeline pipeline;

        public Registry<TSetting> Settings => settings;
        public Registry<TContext> Contexts => contexts;
        public IReadOnlyList<TProcessor> Processors => processors;
        public IPipeline Pipeline => pipeline;

        protected bool Initialized;

        private bool hasUpdateProcessor;
        private bool hasFixedUpdateProcessor;
        private bool hasLateUpdateProcessor;

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            Initialize();
            PerformInvoke(InvokeTiming.Awake);
        }

        protected virtual void Start() => PerformInvoke(InvokeTiming.Start);

        protected virtual void Update()
        {
            if (hasUpdateProcessor)
                PerformInvoke(InvokeTiming.Update);
        }

        protected virtual void FixedUpdate()
        {
            if (hasFixedUpdateProcessor)
                PerformInvoke(InvokeTiming.FixedUpdate);
        }

        protected virtual void LateUpdate()
        {
            if (hasLateUpdateProcessor)
                PerformInvoke(InvokeTiming.LateUpdate);
        }

        protected virtual void OnEnable() => PerformInvoke(InvokeTiming.OnEnable);
        protected virtual void OnDisable() => PerformInvoke(InvokeTiming.OnDisable);
        protected virtual void OnDestroy() => PerformInvoke(InvokeTiming.Destroy);

        #endregion

        #region Initialization

        protected virtual void Initialize()
        {
            if (Initialized) return;
            Initialized = true;

            EnsureCollections();
            ValidateRuntimeDependencies();
            InitializeProcessors();

            pipeline.Initialize(Processors);
        }

        private void InitializeProcessors()
        {
            foreach (var processor in processors)
            {
                if (processor == null) continue;

                processor.Initialize(settings, contexts);

                if (processor.InvokeTiming.HasFlag(InvokeTiming.Update))
                    hasUpdateProcessor = true;
                if (processor.InvokeTiming.HasFlag(InvokeTiming.FixedUpdate))
                    hasFixedUpdateProcessor = true;
                if (processor.InvokeTiming.HasFlag(InvokeTiming.LateUpdate))
                    hasLateUpdateProcessor = true;
            }
        }

        protected virtual void PerformInvoke(InvokeTiming invokeTiming)
        {
            pipeline?.Execute(invokeTiming);
        }

        #endregion

        #region Dependency Validation

        private void CollectDependencies(HashSet<Type> ctx, HashSet<Type> set)
        {
            foreach (var processor in processors)
            {
                if (processor == null) continue;

                foreach (var t in ProcessorDependencyValidator.GetRequiredContexts(processor.GetType()))
                    ctx.Add(t);

                foreach (var t in ProcessorDependencyValidator.GetRequiredSettings(processor.GetType()))
                    set.Add(t);
            }
        }

        private void ValidateProcessorDependencies(HashSet<Type> requiredContexts, HashSet<Type> requiredSettings)
        {
            foreach (var type in requiredContexts)
            {
                if (!typeof(TContext).IsAssignableFrom(type))
                    throw new InvalidOperationException(
                        $"Context '{type.Name}' is not compatible with controller context '{typeof(TContext).Name}'.");
            }

            foreach (var type in requiredSettings)
            {
                if (!typeof(TSetting).IsAssignableFrom(type))
                    throw new InvalidOperationException(
                        $"Setting '{type.Name}' is not compatible with controller setting '{typeof(TSetting).Name}'.");
            }
        }

        protected void ValidateRuntimeDependencies()
        {
            var requiredContexts = new HashSet<Type>();
            var requiredSettings = new HashSet<Type>();

            CollectDependencies(requiredContexts, requiredSettings);
            ValidateProcessorDependencies(requiredContexts, requiredSettings);
        }

        #endregion

        #region Registry

        private void EnsureCollections()
        {
            settings ??= new();
            contexts ??= new();
            processors ??= new();
        }

        private bool SyncRegistry<T>(Registry<T> registry, HashSet<Type> required) where T : class
        {
            bool changed = registry.SerializedObjects.RemoveAll(o => o == null) > 0;

            foreach (var type in required)
            {
                if (registry.Contains(type)) continue;

                registry.Register(type, (T)Activator.CreateInstance(type));
                changed = true;
            }

            return changed;
        }

        private bool RemoveDuplicateRegistryEntries<T>(Registry<T> registry) where T : class
        {
            bool removed = false;
            var seen = new HashSet<Type>();

            for (int i = registry.SerializedObjects.Count - 1; i >= 0; i--)
            {
                var obj = registry.SerializedObjects[i];

                if (obj == null || !seen.Add(obj.GetType()))
                {
                    registry.SerializedObjects.RemoveAt(i);
                    removed = true;
                }
            }

            return removed;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR

        public override object GetSetting() => settings;
        public override object GetContext() => contexts;
        public override object GetProcessor() => processors;

        public override Type SettingType() => typeof(TSetting);
        public override Type ContextType() => typeof(TContext);
        public override Type ProcessorType() => typeof(TProcessor);

        private void OnValidate()
        {
            if (Application.isPlaying) return;

            EnsureCollections();

            bool changed = false;
            changed |= RemoveDuplicateRegistryEntries(contexts);
            changed |= RemoveDuplicateRegistryEntries(settings);

            if (processors.Count == 0)
            {
                ApplyEditorChanges(changed, "Controller Idle");
                return;
            }

            changed |= RemoveDuplicateProcessors();

            var requiredContexts = new HashSet<Type>();
            var requiredSettings = new HashSet<Type>();

            CollectDependencies(requiredContexts, requiredSettings);
            ValidateProcessorDependencies(requiredContexts, requiredSettings);

            changed |= SyncRegistry(contexts, requiredContexts);
            changed |= SyncRegistry(settings, requiredSettings);

            ApplyEditorChanges(changed, "Controller Auto Setup");
        }

        private bool RemoveDuplicateProcessors()
        {
            bool removed = false;
            var seen = new HashSet<Type>();
            var duplicates = new List<Type>();

            for (int i = processors.Count - 1; i >= 0; i--)
            {
                var p = processors[i];
                if (p == null) continue;

                var type = p.GetType();
                if (!seen.Add(type))
                {
                    duplicates.Add(type);
                    processors.RemoveAt(i);
                    removed = true;
                }
            }

            if (duplicates.Count > 0)
            {
                EditorUtility.DisplayDialog(
                    "Duplicate Processor Removed",
                    "Duplicate processors are not allowed:\n\n" +
                    string.Join("\n", duplicates.ConvertAll(t => t.Name)),
                    "OK");
            }

            return removed;
        }

        private void ApplyEditorChanges(bool changed, string undoName)
        {
            if (!changed) return;

            Undo.RegisterCompleteObjectUndo(this, undoName);
            EditorUtility.SetDirty(this);

            if (gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

#endif

        #endregion
    }
}
