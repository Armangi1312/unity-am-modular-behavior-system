using System;
using System.Collections.Generic;

namespace AM.Core.Pipelines
{
    public class BranchPipeline : IBranchPipeline
    {
        public List<IPipeline> NextPipelines { get; } = new();
        public List<(IPipeline Pipeline, Func<bool> Condition)> ConditionPipelines { get; } = new();

        public void Initialize(IReadOnlyList<IProcessor> processors)
        {
            for (int i = 0; i < ConditionPipelines.Count; i++)
                ConditionPipelines[i].Pipeline.Initialize(processors);

            for (int i = 0; i < NextPipelines.Count; i++)
                NextPipelines[i].Initialize(processors);
        }

        public void Execute(InvokeTiming invokeTiming)
        {
            for (int i = 0; i < ConditionPipelines.Count; i++)
            {
                var (pipeline, condition) = ConditionPipelines[i];
                if (condition())
                {
                    pipeline.Execute(invokeTiming);
                    break;
                }
            }

            for (int i = 0; i < NextPipelines.Count; i++)
                NextPipelines[i].Execute(invokeTiming);
        }
    }
}
