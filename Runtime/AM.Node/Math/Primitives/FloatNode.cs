using GraphProcessor;
using System;
using System.Collections.Generic;

namespace AM.Node.Math.Primitives
{
    [Serializable, NodeMenuItem("Primitives/Float")]
    public class FloatNode : PrimitiveNode<float>
    {
        public override string name => "Float";
    }
}