using AM.Core;
using System;

[Serializable]
[RequireSetting(typeof(CameraFollowSetting))]
public class CameraFollowProcessor : CameraProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.LateUpdate;

    private CameraFollowSetting setting;

    public override void Initialize(Registry<ICameraSetting> settingRegistry, Registry<ICameraContext> contextRegistry)
    {
        setting = settingRegistry.Get<CameraFollowSetting>();
    }

    public override void Process()
    {
        setting.Transform.position = setting.Target.position;
    }
}