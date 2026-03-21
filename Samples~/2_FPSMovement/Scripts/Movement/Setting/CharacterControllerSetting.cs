using System;
using UnityEngine;

[Serializable]
public class CharacterControllerSetting : IMovementSetting
{
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
}