using AM.Core;
using AM.Core.Utilities;
using System;
using UnityEngine;

[Serializable]
[RequireSetting(typeof(MoveSetting), typeof(CharacterControllerSetting))]
[RequireContext(typeof(MoveContext), typeof(GroundContext))]
public class MoveProcessor : MovementProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.Update;

    private MoveSetting moveSetting;
    private CharacterControllerSetting characterController;

    private MoveContext moveContext;
    private GroundContext groundContext;

    public override void Initialize(
        IReadOnlyRegistry<IMovementSetting> settingRegistry,
        IReadOnlyRegistry<IMovementContext> contextRegistry)
    {
        moveSetting = settingRegistry.Get<MoveSetting>();
        characterController = settingRegistry.Get<CharacterControllerSetting>();

        moveContext = contextRegistry.Get<MoveContext>();
        groundContext = contextRegistry.Get<GroundContext>();
    }

    public override void Process()
    {
        var controller = characterController.CharacterController;

        Vector3 flatVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);

        moveContext.IsMoving = flatVelocity.normalized.magnitude > 0.3f;

        Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

        moveContext.MoveDirection = inputDirection;

        bool isGrounded = groundContext.IsGrounded;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        float speed;

        if (isSprinting)
        {
            speed = isGrounded ? moveSetting.MoveSpeedOnGround : moveSetting.MoveSpeedOffGround;
        }
        else
        {
            speed = isGrounded ? moveSetting.SprintSpeedOnGround : moveSetting.SprintSpeedOffGround;
        }

        moveContext.Speed = speed;

        moveContext.Acceleration = isGrounded ? moveSetting.AccelerationOnGround : moveSetting.AccelerationInAir;

        moveContext.Deceleration = isGrounded ? moveSetting.DecelerationOnGround : moveSetting.DecelerationInAir;

        float targetAcceleration = inputDirection.magnitude == 0 ? moveContext.Deceleration : moveContext.Acceleration;

        moveContext.MoveVelocity = Vector3.MoveTowards(
            moveContext.MoveVelocity,
            inputDirection * speed,
            targetAcceleration * Time.deltaTime
        );

        controller.Move(controller.transform.TransformDirection(moveContext.MoveVelocity * Time.deltaTime));
    }
}