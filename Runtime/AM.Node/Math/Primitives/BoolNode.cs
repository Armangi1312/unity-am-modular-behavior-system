using GraphProcessor;
using System;

namespace AM.Node.Math.Primitives
{
    [Serializable, NodeMenuItem("Primitives/Bool")]
    public class BoolNode : PrimitiveNode<bool>
    {
        public override string name => "Bool";
    }
}
