using AM.Core;
using AM.Core.Utilities;
using AM.Module;
using System;
using UnityEngine;

[Serializable]
[RequireSetting(typeof(SwingObjectSetting))]
[RequireContext(typeof(SwingObjectContext))]
public class SwingObjectProcessor : LifeCycleProcessor<ISwingSetting, ISwingContext>
{
    public override InvokeTiming InvokeTiming => InvokeTiming.Update;

    private SwingObjectSetting swingObjectSetting;
    private SwingObjectContext swingObjectContext;

    public override void Initialize(IReadOnlyRegistry<ISwingSetting> settingRegistry, IReadOnlyRegistry<ISwingContext> contextRegistry)
    {
        swingObjectSetting = settingRegistry.Get<SwingObjectSetting>();
        swingObjectContext = contextRegistry.Get<SwingObjectContext>();

        swingObjectContext.InitialPosition = swingObjectSetting.Transform.position;
        swingObjectContext.ElapsedTime = swingObjectSetting.Transform.position.magnitude / 10;
    }

    public override void Process()
    {
        swingObjectContext.ElapsedTime += Time.deltaTime;

        float offset = Mathf.Sin(swingObjectContext.ElapsedTime * swingObjectSetting.SwingSpeed);
        Vector3 displacement = swingObjectSetting.Direction * offset;

        swingObjectSetting.Transform.position = displacement + swingObjectContext.InitialPosition;
    }
}
