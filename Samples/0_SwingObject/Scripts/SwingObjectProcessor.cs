using AM.Core;
using AM.Module;
using System;
using UnityEngine;

[Serializable]
[RequireSetting(typeof(SwingObjectSetting))]
[RequireContext(typeof(SwingObjectContext))]
public class SwingObjectProcessor : LifeCycleProcessor<ISetting, IContext>
{
    public override InvokeTiming InvokeTiming => InvokeTiming.Update;

    private SwingObjectSetting swingObjectSetting;
    private SwingObjectContext swingObjectContext;

    public override void Initialize(Registry<ISetting> settingRegistry, Registry<IContext> contextRegistry)
    {
        swingObjectSetting = settingRegistry.Get<SwingObjectSetting>();
        swingObjectContext = contextRegistry.Get<SwingObjectContext>();
    }

    public override void Process()
    {
        swingObjectContext.ElapsedTime += Time.deltaTime;

        float offset = Mathf.Sin(swingObjectContext.ElapsedTime * swingObjectSetting.SwingSpeed);
        Vector3 displacement = swingObjectSetting.Direction * offset;

        swingObjectSetting.TargetTransform.position = displacement + swingObjectSetting.InitialPosition;
    }
}
