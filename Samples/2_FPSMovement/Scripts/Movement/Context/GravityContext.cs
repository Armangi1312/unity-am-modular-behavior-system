using System;
using UnityEngine;

[Serializable]
public class GravityContext : IMovementContext
{
    [field: SerializeField] public float GravityVelocity { get; set; }
}