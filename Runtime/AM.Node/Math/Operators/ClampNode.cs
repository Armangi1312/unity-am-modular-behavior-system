using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Advance/Clamp")]
    public class ClampNode : OperatorNode<float>
    {
        [Input("Value")] public float Value;
        [Input("Min")] public float Min;
        [Input("Max")] public float Max;

        public override string name => "Clamp";
    }
}
