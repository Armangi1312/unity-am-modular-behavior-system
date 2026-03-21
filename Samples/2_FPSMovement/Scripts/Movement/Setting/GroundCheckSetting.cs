using System;
using UnityEngine;

[Serializable]
public class GroundCheckSetting : IMovementSetting
{
    [field: SerializeField] public Transform GroundCheckPosition { get; private set; }
    [field: SerializeField] public LayerMask GroundLayerMask { get; private set; }
    [field: SerializeField] public float GroundCheckSphereRadius { get; private set; }
}