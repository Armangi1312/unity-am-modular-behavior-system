using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/ComparisonNode")]
    public class ComparisonNode : OperatorNode<bool>
    {
        [Input("A")] public object A;
        [Input("B")] public object B;
        
        public ComparisonType ComparisonType;

        public override string name => "Comparison";
    }
}
