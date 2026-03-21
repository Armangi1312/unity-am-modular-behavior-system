using System;

namespace AM.Core.Utilities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class RequireSettingAttribute : Attribute
    {
        public Type[] SettingTypes { get; }

        public RequireSettingAttribute(params Type[] contextType)
        {
            SettingTypes = contextType;
        }
    }
}
