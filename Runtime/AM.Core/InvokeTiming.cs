using System;

namespace AM.Core
{
    [Flags]
    public enum InvokeTiming
    {
        DoNotInvoke = 1 << 0,
        Awake = 1 << 1,
        Start = 1 << 2,
        Update = 1 << 3,
        FixedUpdate = 1 << 4,
        LateUpdate = 1 << 5,
        Destroy = 1 << 6,
        OnEnable = 1 << 7,
        OnDisable = 1 << 8,
    }
}
