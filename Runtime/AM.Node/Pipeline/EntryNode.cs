using GraphProcessor;
using System;

namespace AM.Node.Pipelines
{
    [Serializable]
    [NodeMenuItem("Pipeline/Entry")]
    public class EntryNode : PipelineNode
    {
        [Output("Next")]
        public PipelineFlow NextPipeline;

        public override string name => "Entry";
    }
}
