using System;

namespace AM.Core
{
    [Serializable]
    public abstract class Processor : IProcessor
    {
        public abstract void Initialize(Registry<ISetting> settingRegistry, Registry<IContext> contextRegistry);

        public abstract void Process();
        public abstract InvokeTiming InvokeTiming { get; }
    }
}
