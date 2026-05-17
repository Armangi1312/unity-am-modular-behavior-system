using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Primitives
{
    [Serializable, NodeMenuItem("Primitives/Vector4")]
    public class Vector4Node : PrimitiveNode<Vector4>
    {
        public override string name => "Vector4";
    }
}
