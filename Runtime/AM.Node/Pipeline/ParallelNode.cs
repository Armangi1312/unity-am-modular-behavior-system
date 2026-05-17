using GraphProcessor;
using System;
using UnityEngine;
using System.Collections.Generic;
using AM.Core;

namespace AM.Node.Pipelines
{
    [Serializable]
    [NodeMenuItem("Pipeline/Parallel Invoke")]
    public class ParallelNode : PipelineNode
    {
        [Input("In")]
        public PipelineFlow InputPipeline;

        [SerializeReference] public List<IProcessor> Processors = new();

        [Output("Next")]
        public PipelineFlow NextPipeline;

        public override string name => "Parallel Invoke";
    }
}
