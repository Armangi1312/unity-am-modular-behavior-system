using UnityEngine;

public static class AnimationExtensions
{
    public static void SetValue(this Animator animator, int parameterID, object value)
    {
        switch (value)
        {
            case int intValue:
                animator.SetInteger(parameterID, intValue);
                break;

            case float floatValue:
                animator.SetFloat(parameterID, floatValue);
                break;

            case bool boolValue:
                animator.SetBool(parameterID, boolValue);
                break;

            case null:
                animator.SetTrigger(parameterID);
                break;

            default:
                throw new System.ArgumentException($"No: {value.GetType().Name}");
        }
    }

    public static void SetValue(this Animator animator, string name, object value)
    {
        switch (value)
        {
            case int intValue:
                animator.SetInteger(name, intValue);
                break;

            case float floatValue:
                animator.SetFloat(name, floatValue);
                break;

            case bool boolValue:
                animator.SetBool(name, boolValue);
                break;

            case null:
                animator.SetTrigger(name);
                break;

            default:
                throw new System.ArgumentException($"No: {value.GetType().Name}");
        }
    }

    public static void SetValue(this Animator animator, AnimationParameter parameter, object value)
    {
        switch (parameter.Type)
        {
            case AnimatorControllerParameterType.Bool:
                animator.SetBool(parameter.ParameterID, (bool)value);
                break;
            case AnimatorControllerParameterType.Float:
                animator.SetFloat(parameter.ParameterID, (float)value);
                break;
            case AnimatorControllerParameterType.Int:
                animator.SetInteger(parameter.ParameterID, (int)value);
                break;
            case AnimatorControllerParameterType.Trigger:
                animator.SetTrigger(parameter.ParameterID);
                break;
        }
    }

    public static void SetFloatIfChanged(this Animator animator, AnimationParameter param, ref float cache, float value)
    {
        if (Mathf.Approximately(cache, value)) return;
        cache = value;
        animator.SetValue(param, value);
    }

    public static void SetBoolIfChanged(this Animator animator, AnimationParameter param, ref bool cache, bool value)
    {
        if (cache == value) return;
        cache = value;
        animator.SetValue(param, value);
    }

    public static void SetBoolIfChanged(this Animator animator, AnimationParameter param, bool value)
    {
        animator.SetValue(param, value);
    }
}