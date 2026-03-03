using AM.Module;
using System;

[Serializable]
public abstract class CameraProcessor : LifeCycleProcessor<ICameraSetting, ICameraContext>
{
}
