using System;
using UnityEngine;

[Serializable]
public class GroundCheckSetting : IMovementSetting
{
    [field: SerializeField] public Collider2D GroundCheckCollider { get; private set; }
    [field: SerializeField] public LayerMask GroundLayerMask { get; private set; }
}
