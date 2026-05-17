using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Advance/Min")]
    public class MinNode : OperatorNode<float>
    {
        [Input("A")] public float A;
        [Input("B")] public float B;

        public override string name => "Min";
    }
}
