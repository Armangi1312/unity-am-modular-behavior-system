using AM.Core;
using System;
using UnityEngine;

[Serializable]
public class SwingObjectSetting : ISetting
{
    [field: SerializeField] public Transform TargetTransform { get; private set; }
    [field: SerializeField] public float SwingSpeed { get; private set; }
    [field: SerializeField] public Vector3 Direction { get; private set; }
    [field: SerializeField] public Vector3 InitialPosition { get; set; }
}
