using System;
using UnityEngine;

[Serializable]
public class JumpAnimationContext : IAnimationContext
{
    [field: SerializeField] public bool IsJumped { get; set; }
}
