using AM.Core;

namespace AM.Module
{
    public interface ILifeCycleProcessor : IProcessor
    {
        InvokeTiming InvokeTiming { get; }
    }
}
