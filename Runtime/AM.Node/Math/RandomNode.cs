using GraphProcessor;
using System;

namespace AM.Node.Math
{
    [Serializable, NodeMenuItem("Math/Random")]
    public class RandomNode : MathNode
    {
        [Input("Min")] public object Min;
        [Input("Max")] public object Max;

        [Output("Out")] public object Value;

        public override string name => "Random";
    }
}
