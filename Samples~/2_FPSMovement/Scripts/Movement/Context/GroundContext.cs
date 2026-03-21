
using System;
using UnityEngine;

[Serializable]
public class GroundContext : IMovementContext
{
    [field: SerializeField] public bool IsGrounded { get; set; }
}
