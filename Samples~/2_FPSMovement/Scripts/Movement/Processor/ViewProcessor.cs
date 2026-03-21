
using AM.Core;
using AM.Core.Utilities;
using System;
using UnityEngine;

[Serializable]
[RequireSetting(typeof(ViewSetting))]
public class ViewProcessor : MovementProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.Update;

    private ViewSetting setting;

    public override void Initialize(IReadOnlyRegistry<IMovementSetting> settingRegistry, IReadOnlyRegistry<IMovementContext> contextRegistry)
    {
        setting = settingRegistry.Get<ViewSetting>();
    }

    public override void Process()
    {
        Vector3 rotation = setting.Transform.localEulerAngles;
        rotation.y = setting.CameraTransform.localEulerAngles.y;
        setting.Transform.localEulerAngles = rotation;
    }
}
