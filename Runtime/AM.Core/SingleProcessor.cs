using System;

namespace AM.Core
{
    [Serializable]
    public abstract class SingleProcessor<TSetting, TContext> : IProcessor<TSetting, TContext>
        where TSetting : ISetting
        where TContext : IContext
    {
        public abstract void Initialize(IReadOnlyRegistry<TSetting> settingRegistry, IReadOnlyRegistry<TContext> contextRegistry);
        public abstract void Process();
    }
}