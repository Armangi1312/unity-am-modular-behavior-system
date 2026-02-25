using System;

namespace AM.Core
{
    [Serializable]
    public abstract class Processor<TSetting, TContext> : IProcessor<TSetting, TContext>
        where TSetting : ISetting
        where TContext : IContext
    {
        public abstract void Initialize(Registry<TSetting> settingRegistry, Registry<TContext> contextRegistry);

        public abstract void Process();
    }
}
