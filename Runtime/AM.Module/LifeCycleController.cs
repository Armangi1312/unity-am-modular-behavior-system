using AM.Core;

namespace AM.Module
{
    public class LifeCycleController<TSetting, TContext, TProcessor> : SingleController<TSetting, TContext, TProcessor>
        where TSetting : class, ISetting
        where TContext : class, IContext
        where TProcessor : class, IProcessor<TSetting, TContext>, ILifeCycleProcessor
    {
        protected bool HasUpdateProcessors;
        protected bool HasFixedUpdateProcessors;
        protected bool HasLateUpdateProcessors;

        protected override void Initialize()
        {
            if (Initialized) return;
            Initialized = true;

            ValidateRuntimeDependencies();

            foreach (var processor in processors)
            {
                if (processor == null) continue;

                processor.Initialize(settings, contexts);

                var timing = processor.InvokeTiming;

                HasUpdateProcessors |= (timing & InvokeTiming.Update) != 0;
                HasFixedUpdateProcessors |= (timing & InvokeTiming.FixedUpdate) != 0;
                HasLateUpdateProcessors |= (timing & InvokeTiming.LateUpdate) != 0;
            }
        }

        protected virtual void PerformInvoke(InvokeTiming invokeTiming)
        {
            for (int i = 0; i < processors.Count; i++)
            {
                TProcessor processor = processors[i];

                if (processor == null || (processor.InvokeTiming & invokeTiming) == 0) continue;

                processor.Process();
            }
        }

        protected override void Awake()
        {
            Initialize();

            PerformInvoke(InvokeTiming.Awake);
        }
        private void Start() => PerformInvoke(InvokeTiming.Start);
        private void OnEnable() => PerformInvoke(InvokeTiming.OnEnable);
        private void OnDisable() => PerformInvoke(InvokeTiming.OnDisable);
        private void OnDestroy() => PerformInvoke(InvokeTiming.Destroy);

        private void Update()
        {
            if (HasUpdateProcessors)
                PerformInvoke(InvokeTiming.Update);
        }

        private void FixedUpdate()
        {
            if (HasFixedUpdateProcessors)
                PerformInvoke(InvokeTiming.FixedUpdate);
        }

        private void LateUpdate()
        {
            if (HasLateUpdateProcessors)
                PerformInvoke(InvokeTiming.LateUpdate);
        }
    }
}
