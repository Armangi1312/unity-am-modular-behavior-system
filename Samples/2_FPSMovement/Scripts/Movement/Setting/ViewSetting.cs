using System;
using UnityEngine;

[Serializable]
public class ViewSetting : IMovementSetting
{
    [field: SerializeField] public Transform Transform { get; private set; }
    [field: SerializeField] public Transform CameraTransform { get; private set; }
}
