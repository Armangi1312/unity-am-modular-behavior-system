using System;
using UnityEngine;

[Serializable]
public class MovementAnimationSetting : IAnimationSetting
{
    [field: SerializeField] public MovementController Controller { get; private set; }
}