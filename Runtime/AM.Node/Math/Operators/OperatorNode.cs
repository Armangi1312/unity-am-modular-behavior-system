using GraphProcessor;
using System;

namespace AM.Node.Math.Operators
{
    [Serializable]
    public abstract class OperatorNode<T> : MathNode 
    {
        [Output("Out")] public T Output;
    }
}
