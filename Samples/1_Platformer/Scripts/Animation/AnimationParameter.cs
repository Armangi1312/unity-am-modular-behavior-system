using System;
using UnityEngine;

[Serializable]
public class AnimationParameter : ISerializationCallbackReceiver
{
    [field: SerializeField] public string Name { get; set; }
    [field: SerializeField] public AnimatorControllerParameterType Type { get; set; }
    [NonSerialized] public int ParameterID;
    public AnimationParameter(string name, AnimatorControllerParameterType type)
    {
        Name = name;
        Type = type;
        if (!string.IsNullOrEmpty(name))
            ParameterID = Animator.StringToHash(name);
    }
    public void OnBeforeSerialize()
    {

    }
    public void OnAfterDeserialize()
    {
        if (!string.IsNullOrEmpty(Name))
            ParameterID = Animator.StringToHash(Name);
    }
}