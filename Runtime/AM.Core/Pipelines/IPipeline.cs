using System.Collections.Generic;

namespace AM.Core.Pipelines
{
    public interface IPipeline
    {
        List<IPipeline> NextPipelines { get; }
        void Initialize(IReadOnlyList<IProcessor> processors);
        void Execute(InvokeTiming invokeTiming);
    }
}
