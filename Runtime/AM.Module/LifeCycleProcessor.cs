using AM.Core;
using System;

namespace AM.Module
{
    [Serializable]
    public abstract class LifeCycleProcessor<TSetting, TContext> : SingleProcessor<TSetting, TContext>, ILifeCycleProcessor<TSetting, TContext>
        where TSetting : ISetting
        where TContext : IContext
    {
        public abstract InvokeTiming InvokeTiming { get; }
    }
}
