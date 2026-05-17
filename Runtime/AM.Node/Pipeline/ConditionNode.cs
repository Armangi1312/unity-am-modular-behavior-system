using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Pipelines
{
    [Serializable]
    [NodeMenuItem("Pipeline/Condition")]
    public class ConditionNode : PipelineNode
    {
        [Input("In")]
        public PipelineFlow InputPipeline;

        [Input("Condition")]
        public bool Condition;

        [Output("Next")]
        public PipelineFlow NextPipeline;

        [Output("True")]
        public PipelineFlow TruePipeline;
        [Output("False")]
        public PipelineFlow FalsePipeline;

        public override string name => "Condition";
    }
}
