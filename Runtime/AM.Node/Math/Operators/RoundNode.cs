using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Advance/Round")]
    public class RoundNode : OperatorNode<float>
    {
        [Input("In")] public float Input;

        public override string name => "Round";
    }
}
