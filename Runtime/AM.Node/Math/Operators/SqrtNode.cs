using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Advance/Sqrt")]
    public class SqrtNode : OperatorNode<float>
    {
        [Input("In")] public float Input;

        public override string name => "Sqrt";
    }
}
