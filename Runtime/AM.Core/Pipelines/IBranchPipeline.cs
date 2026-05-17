using System;
using System.Collections.Generic;

namespace AM.Core.Pipelines
{
    public interface IBranchPipeline : IPipeline
    {
        List<(IPipeline Pipeline, Func<bool> Condition)> ConditionPipelines { get; }
    }
}
