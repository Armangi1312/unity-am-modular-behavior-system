using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace AM.Core
{
    public class Controller : MonoBehaviour
    {
        [SerializeReference] private Registry<ISetting> settings = new();
        [SerializeReference] private Registry<IContext> contexts = new();
        [SerializeReference] private List<Processor> processors = new();

        public Registry<ISetting> Settings => settings;
        public Registry<IContext> Contexts => contexts;
        public IReadOnlyList<Processor> Processors => processors;

        private bool initialized;
        private bool hasUpdateProcessors;
        private bool hasFixedUpdateProcessors;
        private bool hasLateUpdateProcessors;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            EnsureCollections();

            bool changed = false;

            if (processors.Count == 0)
            {
                changed |= ClearRegistriesIfNeeded();
                ApplyEditorChanges(changed, "Clear Registries");
                return;
            }

            changed |= RemoveDuplicateProcessors();

            var requiredContexts = new HashSet<Type>();
            var requiredSettings = new HashSet<Type>();

            CollectDependencies(requiredContexts, requiredSettings);

            changed |= SyncRegistry(contexts, requiredContexts);
            changed |= SyncRegistry(settings, requiredSettings);

            ApplyEditorChanges(changed, "Controller Auto Setup");
        }

        private void EnsureCollections()
        {
            settings ??= new();
            contexts ??= new();
            processors ??= new();
        }

        private bool ClearRegistriesIfNeeded()
        {
            if (contexts.SerializedObjects.Count == 0 &&
                settings.SerializedObjects.Count == 0)
                return false;

            contexts.SerializedObjects.Clear();
            settings.SerializedObjects.Clear();
            return true;
        }

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

        private void ApplyEditorChanges(bool changed, string undoName)
        {
            if (!changed) return;

            Undo.RegisterCompleteObjectUndo(this, undoName);
            MarkDirty();
        }
#endif

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

        private bool SyncRegistry<T>(
            Registry<T> registry,
            HashSet<Type> required)
            where T : class
        {
            bool changed = false;

            if (registry.SerializedObjects.RemoveAll(o => o == null) > 0)
                changed = true;

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

#if UNITY_EDITOR
        private void MarkDirty()
        {
            EditorUtility.SetDirty(this);

            if (gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif

        protected void Initialize()
        {
            if (initialized) return;
            initialized = true;

            foreach (var processor in processors)
            {
                if (processor == null) continue;

                processor.Initialize(settings, contexts);

                var timing = processor.InvokeTiming;

                if ((timing & InvokeTiming.Update) != 0)
                    hasUpdateProcessors = true;

                if ((timing & InvokeTiming.FixedUpdate) != 0)
                    hasFixedUpdateProcessors = true;

                if ((timing & InvokeTiming.LateUpdate) != 0)
                    hasLateUpdateProcessors = true;
            }
        }

        private void Awake()
        {
            if (!initialized)
                Initialize();

            PerformInvoke(InvokeTiming.Awake);
        }

        private void Start() => PerformInvoke(InvokeTiming.Start);
        private void OnEnable() => PerformInvoke(InvokeTiming.OnEnable);
        private void OnDisable() => PerformInvoke(InvokeTiming.OnDisable);
        private void OnDestroy() => PerformInvoke(InvokeTiming.OnDestroyed);

        private void Update()
        {
            if (hasUpdateProcessors)
                PerformInvoke(InvokeTiming.Update);
        }

        private void FixedUpdate()
        {
            if (hasFixedUpdateProcessors)
                PerformInvoke(InvokeTiming.FixedUpdate);
        }

        private void LateUpdate()
        {
            if (hasLateUpdateProcessors)
                PerformInvoke(InvokeTiming.LateUpdate);
        }

        protected void PerformInvoke(InvokeTiming timing)
        {
            foreach (var processor in processors)
            {
                if (processor == null) continue;

                if ((processor.InvokeTiming & timing) != 0)
                    processor.Process();
            }
        }
    }
}