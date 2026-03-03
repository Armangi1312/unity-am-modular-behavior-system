using System;
using UnityEngine;

[Serializable]
public class Rigidbody2DSetting : IMovementSetting
{
    [field: SerializeField] public Rigidbody2D Rigidbody2D { get; private set; }
}