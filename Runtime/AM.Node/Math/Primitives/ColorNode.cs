using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Primitives
{
    [Serializable, NodeMenuItem("Primitives/Color")]
    public class ColorNode : PrimitiveNode<Color>
    {
        public override string name => "Color";
    }
}
