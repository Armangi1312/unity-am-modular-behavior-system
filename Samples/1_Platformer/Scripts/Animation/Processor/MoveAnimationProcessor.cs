using AM.Core;
using System;
using UnityEngine;

[Serializable]
[RequireSetting(typeof(MoveAnimationSetting))]
[RequireSetting(typeof(AnimatorSetting))]
[RequireSetting(typeof(MovementAnimationSetting))]
[RequireContext(typeof(MoveAnimationContext))]
public class MoveAnimationProcessor : AnimationProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.Update;

    private MoveAnimationSetting moveAnimationSetting;
    private AnimatorSetting animatorSetting;
    private MovementAnimationSetting movementAnimationSetting;

    private MoveAnimationContext moveAnimationContext;
    private MoveContext moveContext;

    public override void Initialize(
        Registry<IAnimationSetting> settingRegistry,
        Registry<IAnimationContext> contextRegistry)
    {
        moveAnimationSetting = settingRegistry.Get<MoveAnimationSetting>();
        animatorSetting = settingRegistry.Get<AnimatorSetting>();
        movementAnimationSetting = settingRegistry.Get<MovementAnimationSetting>();

        moveAnimationContext = contextRegistry.Get<MoveAnimationContext>();

        moveContext = movementAnimationSetting
            .Controller
            .Contexts
            .Get<MoveContext>();
    }

    public override void Process()
    {
        bool isMoving = moveContext.IsMoving;

        moveAnimationContext.IsMoving = isMoving;

        var animator = animatorSetting.Animator;

        animator.SetBoolIfChanged(
            moveAnimationSetting.MoveParameter,
            isMoving
        );

        if (moveAnimationSetting.SpriteRenderer != null)
        {
            float direction = moveContext.MoveDirection;

            if (Mathf.Abs(direction) > 0.01f)
                moveAnimationSetting.SpriteRenderer.flipX = direction > 0;
        }
    }
}