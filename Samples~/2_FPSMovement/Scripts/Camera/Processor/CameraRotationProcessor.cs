using AM.Core;
using AM.Core.Utilities;
using System;
using UnityEngine;

[Serializable]
[RequireSetting(typeof(CameraRotationSetting))]
[RequireContext(typeof(CameraRotationContext))]
public class CameraRotationProcessor : CameraProcessor
{
    public override InvokeTiming InvokeTiming => InvokeTiming.LateUpdate;

    private CameraRotationSetting setting;
    private CameraRotationContext context;

    public override void Initialize(IReadOnlyRegistry<ICameraSetting> settingRegistry, IReadOnlyRegistry<ICameraContext> contextRegistry)
    {
        setting = settingRegistry.Get<CameraRotationSetting>();
        context = contextRegistry.Get<CameraRotationContext>();
    }

    public override void Process()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * setting.Sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * setting.Sensitivity;

        context.CurrentYRotation += mouseX;
        context.CurrentXRotation -= mouseY;

        context.CurrentXRotation = Mathf.Clamp(
            context.CurrentXRotation,
            setting.MinRotationX,
            setting.MaxRotationX);

        Quaternion targetRotation = Quaternion.Euler(
            context.CurrentXRotation,
            context.CurrentYRotation,
            0f);

        setting.Transform.localRotation = targetRotation;
    }
}