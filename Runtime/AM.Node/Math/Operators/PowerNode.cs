using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Basic/Power")]
    public class PowerNode : OperatorNode<float>
    {
        [Input("Base")] public float Base;
        [Input("Exponent")] public float Exponent;

        public override string name => "Power";
    }
}
