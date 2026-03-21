using System;
using UnityEngine;

[Serializable]
public class MoveContext : IMovementContext
{
    [field: Header("Target")]
    [field: SerializeField] public float Speed { get; set; }
    [field: SerializeField] public float Acceleration { get; set; }
    [field: SerializeField] public float Deceleration { get; set; }

    [field: Header("State")]
    [field: SerializeField] public bool IsMoving { get; set; }

    [field: SerializeField] public float MoveDirection { get; set; }
    [field: SerializeField] public float LinearVelocityX { get; set; }
}
