using System;
using UnityEngine;

[Serializable]
public class AnimatorSetting : IAnimationSetting
{
    [field: SerializeField] public Animator Animator { get; private set; }
}
