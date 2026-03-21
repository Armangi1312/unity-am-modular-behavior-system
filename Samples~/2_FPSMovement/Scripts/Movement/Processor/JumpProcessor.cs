using AM.Core;
using AM.Core.Utilities;
using System;
using UnityEngine;

[Serializable]
[RequireSetting(typeof(JumpSetting), typeof(CharacterControllerSetting))]
[RequireContext(typeof(JumpContext), typeof(GroundContext))]
public class JumpProcessor : MovementProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.Update;

    private JumpSetting jumpSetting;
    private CharacterControllerSetting characterController;

    private JumpContext jumpContext;
    private GroundContext groundContext;

    public override void Initialize(
        IReadOnlyRegistry<IMovementSetting> settingRegistry,
        IReadOnlyRegistry<IMovementContext> contextRegistry)
    {
        jumpSetting = settingRegistry.Get<JumpSetting>();
        characterController = settingRegistry.Get<CharacterControllerSetting>();

        jumpContext = contextRegistry.Get<JumpContext>();
        groundContext = contextRegistry.Get<GroundContext>();
    }

    public override void Process()
    {
        var controller = characterController.CharacterController;

        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
        jumpContext.IsJumped = false;

        if (groundContext.IsGrounded && jumpPressed)
        {
            jumpContext.JumpVelocity = jumpSetting.JumpPower;
            jumpContext.IsJumped = true;
        }

        jumpContext.JumpVelocity = Mathf.MoveTowards(
            jumpContext.JumpVelocity,
            0f,
            Time.deltaTime * jumpContext.JumpVelocity * 5f
        );

        Vector3 jumpMove = (jumpContext.JumpVelocity + controller.velocity.y) * Time.deltaTime * Vector3.up;
        controller.Move(jumpMove);
    }
}