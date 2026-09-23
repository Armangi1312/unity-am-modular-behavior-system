using System.Collections.Generic;

namespace AM.Core.Pipelines
{
    public class ParallelPipeline : IInvokePipeline
    {
        public List<IPipeline> NextPipelines { get; } = new();
        public IReadOnlyList<IProcessor> Processors { get; private set; }

        public void Initialize(IReadOnlyList<IProcessor> processors)
        {
            Processors = processors;

            for (int i = 0; i < NextPipelines.Count; i++)
                NextPipelines[i].Initialize(processors);
        }

        public void Execute(InvokeTiming invokeTiming)
        {
            for (int i = 0; i < Processors.Count; i++)
            {
                var processor = Processors[i];
                if ((processor.InvokeTiming & invokeTiming) != 0)
                    processor.Process();
            }

            for (int i = 0; i < NextPipelines.Count; i++)
                NextPipelines[i].Execute(invokeTiming);
        }
    }
}
