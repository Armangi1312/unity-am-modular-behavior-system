using System;

namespace AM.Core
{
    public interface IProcessor
    {
        InvokeTiming InvokeTiming { get; }
        void Process();
    }

    public interface IProcessor<TSetting, TContext> : IProcessor
        where TSetting : ISetting
        where TContext : IContext
    {
        void Initialize(IReadOnlyRegistry<TSetting> settingRegistry, IReadOnlyRegistry<TContext> contextRegistry);
    }

    [Serializable]
    public abstract class Processor<TSetting, TContext> : IProcessor<TSetting, TContext>
        where TSetting : ISetting
        where TContext : IContext
    {
        public abstract InvokeTiming InvokeTiming { get; }
        public abstract void Initialize(IReadOnlyRegistry<TSetting> settingRegistry, IReadOnlyRegistry<TContext> contextRegistry);
        public abstract void Process();
    }
}