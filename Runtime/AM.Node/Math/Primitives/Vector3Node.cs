using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Math.Primitives
{
    [Serializable, NodeMenuItem("Primitives/Vector3")]
    public class Vector3Node : PrimitiveNode<Vector3>
    {
        public override string name => "Vector3";
    }
}
