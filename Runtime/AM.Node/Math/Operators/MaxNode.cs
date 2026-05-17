using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Advance/Max")]
    public class MaxNode : OperatorNode<float>
    {
        [Input("A")] public float A;
        [Input("B")] public float B;

        public override string name => "Max";
    }
}
