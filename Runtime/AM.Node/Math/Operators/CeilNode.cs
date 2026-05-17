using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Advance/Ceil")]
    public class CeilNode : OperatorNode<float>
    {
        [Input("In")] public float Input;

        public override string name => "Ceil";
    }
}
