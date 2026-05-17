using GraphProcessor;
using System;

namespace AM.Node.Data
{
    [Serializable]
    public abstract class DataNode : BaseNode
    {
        [Output("Value")] public object Value;
    }
}
