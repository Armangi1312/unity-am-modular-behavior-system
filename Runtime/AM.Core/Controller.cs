using System;
using UnityEngine;

namespace AM.Core
{
    public abstract class Controller : MonoBehaviour
    {
#if UNITY_EDITOR
        public abstract Type SettingType();
        public abstract Type ContextType();
        public abstract Type ProcessorType();

        public abstract object GetSetting();
        public abstract object GetContext();
        public abstract object GetProcessor();
#endif
    }
}
