using System;
using UnityEngine;

[Serializable]
public class JumpSetting : IMovementSetting
{
    [field: SerializeField] public float JumpPower { get; private set; }
}