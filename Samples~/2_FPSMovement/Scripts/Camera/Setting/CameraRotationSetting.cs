using System;
using UnityEngine;

[Serializable]
public class CameraRotationSetting : ICameraSetting
{
    [field: SerializeField] public float Sensitivity { get; private set; }
    [field: SerializeField] public float Smooth { get; private set; }

    [field: SerializeField] public float MaxRotationX { get; private set; }
    [field: SerializeField] public float MinRotationX { get; private set; }

    [field: SerializeField] public Transform Transform { get; private set; }
}