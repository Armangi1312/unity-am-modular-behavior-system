using System;
using UnityEngine;

[Serializable]
public class SwingObjectSetting : ISwingSetting
{
    [field: SerializeField] public Transform Transform { get; private set; }
    [field: SerializeField] public float SwingSpeed { get; private set; }
    [field: SerializeField] public Vector3 Direction { get; private set; }
}
