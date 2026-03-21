using AM.Module;
using System;

[Serializable]
public abstract class MovementProcessor : LifeCycleProcessor<IMovementSetting, IMovementContext>
{
}
