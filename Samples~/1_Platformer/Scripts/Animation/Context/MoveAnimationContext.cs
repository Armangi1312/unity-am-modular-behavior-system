using System;
using UnityEngine;

[Serializable]
public class MoveAnimationContext : IAnimationContext
{
    [field: SerializeField] public bool IsMoving { get; set; }
}
