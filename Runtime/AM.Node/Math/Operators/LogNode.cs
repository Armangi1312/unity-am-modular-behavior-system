using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Advance/Log")]
    public class LogNode : OperatorNode<float>
    {
        [Input("In")] public float Input;

        public override string name => "Log";
    }
}
