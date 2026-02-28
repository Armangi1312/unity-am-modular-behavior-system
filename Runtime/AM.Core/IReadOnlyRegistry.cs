using System;
using System.Collections.Generic;
using System.Text;

namespace AM.Core
{
    public interface IReadOnlyRegistry<TTarget>
    {
        T Get<T>() where T : TTarget;
        bool TryGet<T>(out T value) where T : TTarget;
    }
}
