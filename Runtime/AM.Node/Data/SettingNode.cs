using AM.Core;
using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Data
{
    [Serializable, NodeMenuItem("Data/Get Setting")]
    public class SettingNode : DataNode
    {
        [SerializeReference] public ISetting Setting;

        public override string name => "Get Setting";
    }
}
