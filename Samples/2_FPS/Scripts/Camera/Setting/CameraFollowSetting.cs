using System;
using UnityEngine;

[Serializable]
public class CameraFollowSetting : ICameraSetting
{
    [field: SerializeField] public Transform Target { get; private set; }
    [field: SerializeField] public Transform Transform { get; private set; }
}
