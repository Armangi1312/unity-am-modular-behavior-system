
using System;
using UnityEngine;

[Serializable]
public class CameraRotationContext : ICameraContext
{
    [field: SerializeField] public float CurrentXRotation { get; set; }
    [field: SerializeField] public float CurrentYRotation { get; set; }
}