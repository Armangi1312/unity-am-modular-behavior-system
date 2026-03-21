using System;
using UnityEngine;

[Serializable]
public class MoveContext : IMovementContext
{
    [field: SerializeField] public Vector3 MoveDirection { get; set; }
    [field: SerializeField] public Vector3 MoveVelocity { get; set; }
    [field: SerializeField] public float Speed { get; set; }
    [field: SerializeField] public float Acceleration { get; set; }
    [field: SerializeField] public float Deceleration { get; set; }
    [field: Space]
    [field: SerializeField] public bool IsMoving { get; set; }
}
