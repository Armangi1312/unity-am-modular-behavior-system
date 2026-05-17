using GraphProcessor;
using System;

namespace AM.Node.Math.Primitives
{
    [Serializable]
    public abstract class PrimitiveNode<T> : MathNode where T : struct
    {
        [Output("Out")]
        public T Output;

        public T Input;
    }
}