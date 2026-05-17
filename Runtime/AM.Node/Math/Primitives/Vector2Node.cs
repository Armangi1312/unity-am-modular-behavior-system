using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Primitives
{
    [Serializable, NodeMenuItem("Primitives/Vector2")]
    public class Vector2Node : PrimitiveNode<Vector2>
    {
        public override string name => "Vector2";
    }
}
