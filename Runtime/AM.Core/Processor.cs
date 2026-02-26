using System;
using UnityEngine;

namespace AM.Core
{
    [Serializable]
    public abstract class Processor<TSetting, TContext>
        : IProcessor<TSetting, TContext>
        where TSetting : ISetting
        where TContext : IContext
    {
        public abstract void Initialize(
            Registry<TSetting> settingRegistry,
            Registry<TContext> contextRegistry);

        void IProcessor.Initialize(object settingRegistry, object contextRegistry)
        {
            Debug.Log("Initialized");

            Initialize((Registry<TSetting>)settingRegistry, (Registry<TContext>)contextRegistry);
        }

        public abstract void Process();
    }
}