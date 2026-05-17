using AM.Core;
using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Data
{
    [Serializable, NodeMenuItem("Data/Get Context")]
    public class ContextNode : DataNode
    {
        [SerializeReference] public IContext Context;

        public override string name => "Get Context";
    }
}
