using AM.Core;
using AM.Core.Utilities;
using System;
using UnityEngine;

[Serializable]
[RequireSetting(typeof(JumpSetting))]
[RequireSetting(typeof(Rigidbody2DSetting))]
[RequireContext(typeof(JumpContext))]
[RequireContext(typeof(GroundContext))]
public class JumpProcessor : MovementProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.Update;

    private JumpSetting setting;
    private Rigidbody2DSetting rigidbodySetting;

    private JumpContext context;
    private GroundContext groundContext;

    public override void Initialize(IReadOnlyRegistry<IMovementSetting> settingRegistry, IReadOnlyRegistry<IMovementContext> contextRegistry)
    {
        setting = settingRegistry.Get<JumpSetting>();
        rigidbodySetting = settingRegistry.Get<Rigidbody2DSetting>();

        context = contextRegistry.Get<JumpContext>();
        groundContext = contextRegistry.Get<GroundContext>();
    }

    public override void Process()
    {
        context.IsJumped = false;

        var rigidBody = rigidbodySetting.Rigidbody2D;

        context.IsJumpPressed = Input.GetButtonDown("Jump");

        if (context.IsJumpPressed && groundContext.IsGrounded)
        {
            rigidBody.linearVelocityY = setting.JumpPower;
            context.IsJumped = true;
        }

        if (Input.GetButtonUp("Jump") && rigidBody.linearVelocityY > 0)
        {
            rigidBody.linearVelocityY *= setting.JumpCutFactor;
        }
    }
}
