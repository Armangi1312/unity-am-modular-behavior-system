using System;
using UnityEngine;

[Serializable]
public class GroundContext : IMovementContext
{
    [field: Header("State")]
    [field: SerializeField] public bool IsGrounded { get; set; }

    public Action<bool> OnGroundStateChanged { get; private set; }
}
