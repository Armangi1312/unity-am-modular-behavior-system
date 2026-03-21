using System;
using UnityEngine;

[Serializable]
public class MoveSetting : IMovementSetting
{
    [field: Header("Speed Settings")]
    [field: SerializeField] public float MoveSpeedOnGround { get; private set; }
    [field: SerializeField] public float MoveSpeedOffGround { get; private set; }
    [field: Space]
    [field: SerializeField] public float SprintSpeedOnGround { get; private set; }
    [field: SerializeField] public float SprintSpeedOffGround { get; private set; }

    [field: Header("Physic Settings")]
    [field: SerializeField] public float AccelerationOnGround { get; private set; }
    [field: SerializeField] public float AccelerationInAir { get; private set; }
    [field: Space]
    [field: SerializeField] public float DecelerationOnGround { get; private set; }
    [field: SerializeField] public float DecelerationInAir { get; private set; }
}
