using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Advance/Abs")]
    public class AbsNode : OperatorNode<float>
    {
        [Input("In")] public float Input;

        public override string name => "Abs";
    }
}
