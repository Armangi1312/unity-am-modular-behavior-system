using AM.Core;

namespace AM.Module
{
    public abstract class LifeCycleProcessor<TSetting, TContext> : Processor<TSetting, TContext>, ILifeCycleProcessor<TSetting, TContext>
        where TSetting : ISetting
        where TContext : IContext
    {
        public abstract InvokeTiming InvokeTiming { get; }
    }
}
