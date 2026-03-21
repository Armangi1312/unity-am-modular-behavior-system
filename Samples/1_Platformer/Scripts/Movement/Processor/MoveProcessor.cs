using AM.Core;
using AM.Core.Utilities;
using System;
using UnityEngine;

[Serializable]
[RequireSetting(typeof(MoveSetting))]
[RequireSetting(typeof(Rigidbody2DSetting))]
[RequireContext(typeof(MoveContext))]
[RequireContext(typeof(GroundContext))]
public class MoveProcessor : MovementProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.FixedUpdate;

    private MoveSetting setting;
    private Rigidbody2DSetting rigidBody2DSetting;

    private MoveContext context;
    private GroundContext groundContext;

    public override void Initialize(IReadOnlyRegistry<IMovementSetting> settingRegistry, IReadOnlyRegistry<IMovementContext> contextRegistry)
    {
        setting = settingRegistry.Get<MoveSetting>();
        rigidBody2DSetting = settingRegistry.Get<Rigidbody2DSetting>();

        context = contextRegistry.Get<MoveContext>();
        groundContext = contextRegistry.Get<GroundContext>();
    }

    public override void Process()
    {
        bool grounded = groundContext.IsGrounded;

        context.Speed = grounded ? setting.MoveSpeedOnGround : setting.MoveSpeedOffGround;
        context.Acceleration = grounded ? setting.AccelerationOnGround : setting.AccelerationOffGround;
        context.Deceleration = grounded ? setting.DecelerationOnGround : setting.DecelerationOffGround;

        context.MoveDirection = Input.GetAxisRaw("Horizontal");

        var rigidBody = rigidBody2DSetting.Rigidbody2D;

        if (context.MoveDirection != 0)
        {
            rigidBody.AddForceX(context.MoveDirection * context.Acceleration);
        }
        else
        {
            rigidBody.linearVelocityX = Mathf.MoveTowards(
                rigidBody.linearVelocityX,
                0,
                context.Deceleration * Time.fixedDeltaTime
            );
        }

        rigidBody.linearVelocityX = Mathf.Clamp(
            rigidBody.linearVelocityX,
            -context.Speed,
            context.Speed
        );

        context.IsMoving = Mathf.Abs(rigidBody.linearVelocityX) > 0.01f;
        context.LinearVelocityX = rigidBody.linearVelocityX;
    }
}