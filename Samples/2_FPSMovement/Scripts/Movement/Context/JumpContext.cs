using System;
using UnityEngine;

[Serializable]
public class JumpContext : IMovementContext
{
    [field: SerializeField] public bool IsJumpPressed { get; set; }
    [field: SerializeField] public bool IsJumped { get; set; }
    [field: SerializeField] public float JumpVelocity { get; set; }
}