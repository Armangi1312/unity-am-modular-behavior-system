using System;
using UnityEngine;

[Serializable]
public class JumpContext : IMovementContext
{
    [field: Header("State")]
    [field: SerializeField] public bool IsJumped { get; set; }
    [field: SerializeField] public bool IsJumpPressed { get; set; }
}
