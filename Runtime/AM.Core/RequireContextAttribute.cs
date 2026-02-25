using System;

namespace AM.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class RequireContextAttribute : Attribute
    {
        public Type[] ContextTypes { get; }

        public RequireContextAttribute(params Type[] contextType)
        {
            ContextTypes = contextType;
        }
    }
}
