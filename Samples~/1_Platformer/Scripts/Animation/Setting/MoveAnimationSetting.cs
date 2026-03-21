using System;
using UnityEngine;

[Serializable]
public class MoveAnimationSetting : IAnimationSetting
{
    [field: SerializeField] public AnimationParameter MoveParameter { get; private set; }
    [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
}
