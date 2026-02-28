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
        where TProcessor : class, IProcessor
    {
        [SerializeReference] protected Registry<TSetting> settings = new();
        [SerializeReference] protected Registry<TContext> contexts = new();
        [SerializeReference] protected List<TProcessor> processors = new();

        public Registry<TSetting> Settings => settings;
        public Registry<TContext> Contexts => contexts;

        public IReadOnlyList<TProcessor> Processors => processors;

        protected bool Initialized;

        #region Editor Code

#if UNITY_EDITOR

        public override object GetSetting() => settings;
        public override object GetContext() => contexts;
        public override object GetProcessor() => processors;

        public override Type SettingType() => typeof(TSetting);
        public override Type ContextType() => typeof(TContext);
        public override Type ProcessorType() => typeof(TProcessor);

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            EnsureCollections();

            bool changed = false;

            // 🔹 Registry 중복 및 null 정리
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
#endif

        private void EnsureCollections()
        {
            settings ??= new();
            contexts ??= new();
            processors ??= new();
        }

        #endregion

        #region Dependency Collection

        private void CollectDependencies(
            HashSet<Type> ctx,
            HashSet<Type> set)
        {
            foreach (var processor in processors)
            {
                if (processor == null) continue;

                foreach (var t in ProcessorDependencyValidator
                         .GetRequiredContexts(processor.GetType()))
                    ctx.Add(t);

                foreach (var t in ProcessorDependencyValidator
                         .GetRequiredSettings(processor.GetType()))
                    set.Add(t);
            }
        }

        #endregion

        #region Validation

        private void ValidateProcessorDependencies(
            HashSet<Type> requiredContexts,
            HashSet<Type> requiredSettings)
        {
            foreach (var type in requiredContexts)
            {
                if (!typeof(TContext).IsAssignableFrom(type))
                {
                    throw new InvalidOperationException(
                        $"Context '{type.Name}' is not compatible with controller context '{typeof(TContext).Name}'.");
                }
            }

            foreach (var type in requiredSettings)
            {
                if (!typeof(TSetting).IsAssignableFrom(type))
                {
                    throw new InvalidOperationException(
                        $"Setting '{type.Name}' is not compatible with controller setting '{typeof(TSetting).Name}'.");
                }
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

        #region Registry Sync

        private bool SyncRegistry<T>(
            Registry<T> registry,
            HashSet<Type> required)
            where T : class
        {
            bool changed = false;

            // null 정리만 수행
            if (registry.SerializedObjects.RemoveAll(o => o == null) > 0)
                changed = true;

            // 필요한 타입이 없으면 추가
            foreach (var type in required)
            {
                if (!registry.Contains(type))
                {
                    var instance = Activator.CreateInstance(type);
                    registry.Register(type, (T)instance);
                    changed = true;
                }
            }

            return changed;
        }

        private bool RemoveDuplicateRegistryEntries<T>(Registry<T> registry)
    where T : class
        {
            bool removed = false;
            var seen = new HashSet<Type>();

            for (int i = registry.SerializedObjects.Count - 1; i >= 0; i--)
            {
                var obj = registry.SerializedObjects[i];

                if (obj == null)
                {
                    registry.SerializedObjects.RemoveAt(i);
                    removed = true;
                    continue;
                }

                var type = obj.GetType();

                if (!seen.Add(type))
                {
                    registry.SerializedObjects.RemoveAt(i);
                    removed = true;
                }
            }

            return removed;
        }

        #endregion

        #region Processor Management

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

#if UNITY_EDITOR
            if (duplicates.Count > 0)
            {
                EditorUtility.DisplayDialog(
                    "Duplicate Processor Removed",
                    "Duplicate processors are not allowed:\n\n" +
                    string.Join("\n", duplicates.ConvertAll(t => t.Name)),
                    "OK");
            }
#endif
            return removed;
        }

        #endregion

        #region Initialization & Execution

        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            if (Initialized) return;
            Initialized = true;

            ValidateRuntimeDependencies();

            foreach (var processor in processors)
            {
                if (processor == null) continue;
                processor.Initialize(settings, contexts);
            }
        }

        protected virtual void PerformInvoke()
        {
            for (int i = 0; i < processors.Count; i++)
            {
                TProcessor processor = processors[i];

                if (processor == null) continue;

                processor.Process();
            }
        }

        #endregion

#if UNITY_EDITOR
        private void ApplyEditorChanges(bool changed, string undoName)
        {
            if (!changed) return;

            Undo.RegisterCompleteObjectUndo(this, undoName);
            MarkDirty();
        }

        private void MarkDirty()
        {
            EditorUtility.SetDirty(this);

            if (gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}