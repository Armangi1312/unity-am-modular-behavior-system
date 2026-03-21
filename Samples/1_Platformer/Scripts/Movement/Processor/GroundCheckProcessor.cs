using AM.Core;
using AM.Core.Utilities;
using System;
using UnityEngine;

[Serializable]
[RequireSetting(typeof(GroundCheckSetting))]
[RequireContext(typeof(GroundContext))]
public class GroundCheckProcessor : MovementProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.FixedUpdate;

    private ContactFilter2D groundFilter;
    private readonly Collider2D[] overlapResults = new Collider2D[1];

    private GroundCheckSetting setting;
    private GroundContext context;

    public override void Initialize(IReadOnlyRegistry<IMovementSetting> settingRegistry, IReadOnlyRegistry<IMovementContext> contextRegistry)
    {
        setting = settingRegistry.Get<GroundCheckSetting>();
        context = contextRegistry.Get<GroundContext>();

        groundFilter = new ContactFilter2D
        {
            useTriggers = false
        };
        groundFilter.SetLayerMask(setting.GroundLayerMask);
    }

    public override void Process()
    {
        int hitCount = setting.GroundCheckCollider.Overlap(groundFilter, overlapResults);

        bool isGrounded = hitCount > 0;
        context.IsGrounded = isGrounded;
    }
}