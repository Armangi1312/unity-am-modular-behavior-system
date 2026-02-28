using AM.Core;
using UnityEngine;

namespace AM.Module
{
    public class LifeCycleController<TSetting, TContext> : Controller<TSetting, TContext, IProcessor>
        where TSetting : class, ISetting
        where TContext : class, IContext
    {
        protected override void PerformInvoke()
        {
            base.PerformInvoke();
        }
    }
}
