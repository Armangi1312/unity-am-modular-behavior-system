using AM.Core;
using AM.Core.Utilities;
using System;
using UnityEngine;

[Serializable]
[RequireSetting(typeof(GravitySetting), typeof(CharacterControllerSetting))]
[RequireContext(typeof(GravityContext), typeof(GroundContext))]
public class GravityProcessor : MovementProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.Update;

    private GravitySetting gravitySetting;
    private CharacterControllerSetting characterController;

    private GravityContext gravityContext;
    private GroundContext groundContext;

    public override void Initialize(
        IReadOnlyRegistry<IMovementSetting> settingRegistry,
        IReadOnlyRegistry<IMovementContext> contextRegistry)
    {
        gravitySetting = settingRegistry.Get<GravitySetting>();
        characterController = settingRegistry.Get<CharacterControllerSetting>();

        gravityContext = contextRegistry.Get<GravityContext>();
        groundContext = contextRegistry.Get<GroundContext>();
    }

    public override void Process()
    {
        var controller = characterController.CharacterController;

        gravityContext.GravityVelocity += gravitySetting.GravityScale * Time.deltaTime;

        if (groundContext.IsGrounded && gravityContext.GravityVelocity < -2f)
        {
            gravityContext.GravityVelocity = -2f;
        }

        Vector3 gravityMove = gravityContext.GravityVelocity * Time.deltaTime * Vector3.up;


        controller.Move(gravityMove);
    }
}