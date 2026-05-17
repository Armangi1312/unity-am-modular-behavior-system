using AM.Core.Utilities;
using GraphProcessor;
using System;
using UnityEngine;

namespace AM.Node.Data
{
    [Serializable, NodeMenuItem("Data/Get Data")]
    public class ComponentDataNode : DataNode
    {
        [SerializeField] private SceneObjectReference<UnityEngine.Object> targetReference = new();

        public UnityEngine.Object Target
        {
            get => targetReference.Value;
            set => targetReference.Value = value;
        }

        public string ComponentTypeName;
        public string Path;

        public override string name => "Get Data";

#if UNITY_EDITOR
        public void Resolve() => targetReference.Resolve();
#endif
    }
}
