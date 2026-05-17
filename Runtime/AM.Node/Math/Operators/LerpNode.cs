using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Advance/Lerp")]
    public class LerpNode : OperatorNode<float>
    {
        [Input("A")] public float A;
        [Input("B")] public float B;
        [Input("T")] public float T;
        public override string name => "Lerp";
    }
}
