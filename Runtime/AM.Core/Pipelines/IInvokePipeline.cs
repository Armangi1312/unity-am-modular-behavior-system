using System.Collections.Generic;

namespace AM.Core.Pipelines
{
    public interface IInvokePipeline : IPipeline
    {
        IReadOnlyList<IProcessor> Processors { get; }
    }
}
