using AM.Core;

namespace AM.Module
{
    public interface ILifeCycleProcessor : IProcessor
    {
        InvokeTiming InvokeTiming { get; }
    }

    public interface ILifeCycleProcessor<TSetting, TContext> : ILifeCycleProcessor, IProcessor<TSetting, TContext>
        where TSetting : ISetting
        where TContext : IContext
    {
    }
}
