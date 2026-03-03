using AM.Core;
using System;
using UnityEngine;

[Serializable]
[RequireSetting(typeof(JumpAnimationSetting))]
[RequireSetting(typeof(AnimatorSetting))]
[RequireSetting(typeof(MovementAnimationSetting))]
[RequireContext(typeof(JumpAnimationContext))]
public class JumpAnimationProcessor : AnimationProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.Update;

    private JumpAnimationSetting jumpAnimationSetting;
    private AnimatorSetting animatorSetting;
    private MovementAnimationSetting movementAnimationSetting;

    private JumpAnimationContext jumpAnimationContext;

    private GroundContext groundContext;

    public override void Initialize(
        Registry<IAnimationSetting> settingRegistry,
        Registry<IAnimationContext> contextRegistry)
    {
        jumpAnimationSetting = settingRegistry.Get<JumpAnimationSetting>();
        animatorSetting = settingRegistry.Get<AnimatorSetting>();
        movementAnimationSetting = settingRegistry.Get<MovementAnimationSetting>();

        jumpAnimationContext = contextRegistry.Get<JumpAnimationContext>();

        var controllerContexts = movementAnimationSetting.Controller.Contexts;

        groundContext = controllerContexts.Get<GroundContext>();
    }

    public override void Process()
    {
        bool isAirborne = !groundContext.IsGrounded;

        jumpAnimationContext.IsJumped = isAirborne;

        var animator = animatorSetting.Animator;

        animator.SetBoolIfChanged(
            jumpAnimationSetting.JumpParameter,
            isAirborne
        );
    }
}