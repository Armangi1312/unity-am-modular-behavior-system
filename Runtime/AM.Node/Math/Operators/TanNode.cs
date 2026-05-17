using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Advance/Tan")]
    public class TanNode : OperatorNode<float>
    {
        [Input("In")] public float Input;

        public override string name => "Tan";
    }
}
