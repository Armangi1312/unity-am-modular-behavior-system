using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Advance/Cos")]
    public class CosNode : OperatorNode<float>
    {
        [Input("In")] public float Input;

        public override string name => "Cos";
    }
}
