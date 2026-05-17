using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Advance/Floor")]
    public class FloorNode : OperatorNode<float>
    {
        [Input("In")] public float Input;

        public override string name => "Floor";
    }
}
