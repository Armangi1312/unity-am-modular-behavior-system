using System;
using UnityEngine;

[Serializable]
public class GravitySetting : IMovementSetting
{
    [field: SerializeField] public float GravityScale { get; private set; }
}
