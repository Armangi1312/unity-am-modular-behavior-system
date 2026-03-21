using AM.Core;
using AM.Core.Utilities;
using System;
using UnityEngine;

[Serializable]
[RequireSetting(typeof(GroundCheckSetting))]
[RequireContext(typeof(GroundContext))]
public class GroundCheckProcessor : MovementProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.Update;

    private GroundCheckSetting setting;
    private GroundContext context;

    public override void Initialize(IReadOnlyRegistry<IMovementSetting> settingRegistry, IReadOnlyRegistry<IMovementContext> contextRegistry)
    {
        setting = settingRegistry.Get<GroundCheckSetting>();
        context = contextRegistry.Get<GroundContext>();
    }

    public override void Process()
    {
        context.IsGrounded = Physics.CheckSphere(setting.GroundCheckPosition.position, setting.GroundCheckSphereRadius, setting.GroundLayerMask);
    }
}