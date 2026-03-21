using System;
using System.Collections.Generic;
using System.Reflection;

namespace AM.Core.Utilities
{
    public static class ProcessorDependencyValidator
    {
        private static readonly Dictionary<Type, Type[]> contextCache = new();
        private static readonly Dictionary<Type, Type[]> settingCache = new();

        public static Type[] GetRequiredContexts(Type processorType)
        {
            if (contextCache.TryGetValue(processorType, out var cached))
                return cached;

            var attrs = processorType.GetCustomAttributes<RequireContextAttribute>(true);
            var list = new List<Type>();

            foreach (var attr in attrs)
            {
                foreach (var type in attr.ContextTypes)
                {
                    if (!typeof(IContext).IsAssignableFrom(type))
                        throw new InvalidOperationException($"{type.Name} must implement IContext.");

                    list.Add(type);
                }
            }

            var result = list.ToArray();
            contextCache[processorType] = result;
            return result;
        }

        public static Type[] GetRequiredSettings(Type processorType)
        {
            if (settingCache.TryGetValue(processorType, out var cached))
                return cached;

            var attrs = processorType.GetCustomAttributes<RequireSettingAttribute>(true);
            var list = new List<Type>();

            foreach (var attr in attrs)
            {
                foreach (var type in attr.SettingTypes)
                {
                    if (!typeof(ISetting).IsAssignableFrom(type))
                        throw new InvalidOperationException($"{type.Name} must implement ISetting.");

                    list.Add(type);
                }
            }

            var result = list.ToArray();
            settingCache[processorType] = result;
            return result;
        }
    }
}