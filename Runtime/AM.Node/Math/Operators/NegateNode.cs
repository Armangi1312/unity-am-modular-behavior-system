using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Advance/Negate")]
    public class NegateNode : OperatorNode<float>
    {
        [Input("In")] public float Input;

        public override string name => "Negate";
    }
}
