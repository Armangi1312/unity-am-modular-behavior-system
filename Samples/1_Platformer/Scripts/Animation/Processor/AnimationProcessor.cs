using AM.Module;
using System;

[Serializable]
public abstract class AnimationProcessor : LifeCycleProcessor<IAnimationSetting, IAnimationContext>
{
}