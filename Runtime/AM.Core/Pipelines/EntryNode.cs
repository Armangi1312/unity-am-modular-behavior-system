using System.Collections.Generic;

namespace AM.Core.Pipelines
{
    public class EntryPipeline : IPipeline
    {
        public List<IPipeline> NextPipelines { get; } = new();

        public void Initialize(IReadOnlyList<IProcessor> processors)
        {
            for (int i = 0; i < NextPipelines.Count; i++)
                NextPipelines[i].Initialize(processors);
        }

        public void Execute(InvokeTiming invokeTiming)
        {
            for (int i = 0; i < NextPipelines.Count; i++)
                NextPipelines[i].Execute(invokeTiming);
        }
    }
}