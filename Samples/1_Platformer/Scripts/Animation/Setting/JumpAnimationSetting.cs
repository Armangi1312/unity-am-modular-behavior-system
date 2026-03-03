using System;
using UnityEngine;

[Serializable]
public class JumpAnimationSetting : IAnimationSetting
{
    [field: SerializeField] public AnimationParameter JumpParameter { get; private set; }
}
