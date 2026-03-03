using System;
using UnityEngine;

[Serializable]
public class MoveSetting : IMovementSetting
{
    [field: Header("Speed")]
    [field: SerializeField] public float MoveSpeedOnGround { get; private set; }
    [field: SerializeField] public float MoveSpeedOffGround { get; private set; }

    [field: Header("Control")]
    [field: SerializeField] public float AccelerationOnGround { get; private set; }
    [field: SerializeField] public float AccelerationOffGround { get; private set; }
    [field: Space]
    [field: SerializeField] public float DecelerationOnGround { get; private set; }
    [field: SerializeField] public float DecelerationOffGround { get; private set; }
}