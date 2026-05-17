using GraphProcessor;
using System;

namespace AM.Node.Math.Primitives
{
    [Serializable, NodeMenuItem("Primitives/Int")]
    public class IntNode : PrimitiveNode<int>
    {
        public override string name => "Int";
    }
}
