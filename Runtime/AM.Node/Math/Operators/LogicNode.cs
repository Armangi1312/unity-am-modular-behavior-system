using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Operators
{
    [Serializable, NodeMenuItem("Math/Logic")]
    public class LogicNode : OperatorNode<bool>
    {
        [Input("A")] public bool A;
        [Input("B")] public bool B;

        public LogicalOperator ComparisonType;

        public override string name => "Logic";
    }
}
