using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace AM.Core.Utilities
{
    [InitializeOnLoad]
    public static class SettingCache
    {
        public static readonly IReadOnlyList<Type> SettingTypes;

        private static readonly Dictionary<Type, List<Type>> filterCache = new();

        private static readonly object cacheLock = new();

        private static readonly List<Type> emptyList = new(0);

        static SettingCache()
        {
            SettingTypes = FindSettingTypes();

            AssemblyReloadEvents.afterAssemblyReload += () =>
            {
                lock (cacheLock)
                {
                    filterCache.Clear();
                }
            };
        }

        public static List<Type> GetFilteredSettingTypes(Type targetType)
        {
            if (targetType == null)
                return emptyList;

            lock (cacheLock)
            {
                if (filterCache.TryGetValue(targetType, out var cached))
                    return cached;

                var result = BuildFilteredList(targetType);

                filterCache[targetType] = result;
                return result;
            }
        }

        private static List<Type> BuildFilteredList(Type targetType)
        {
            var result = new List<Type>();

            bool isGenericTarget = targetType.IsGenericTypeDefinition;

            for (int i = 0; i < SettingTypes.Count; i++)
            {
                var settingType = SettingTypes[i];

                if (!isGenericTarget && targetType.IsAssignableFrom(settingType))
                {
                    result.Add(settingType);
                    continue;
                }

                foreach (var iface in settingType.GetInterfaces())
                {
                    if (!iface.IsGenericType)
                        continue;

                    if (iface.GetGenericTypeDefinition() == targetType)
                    {
                        result.Add(settingType);
                        break;
                    }
                }
            }

            return result;
        }

        private static List<Type> FindSettingTypes()
        {
            var result = new List<Type>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var asm in assemblies)
            {
                if (asm.IsDynamic)
                    continue;

                Type[] types;

                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray();
                }

                foreach (var type in types)
                {
                    if (!IsValidSetting(type))
                        continue;

                    result.Add(type);
                }
            }

            result.Sort((a, b) =>
                string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            return result;
        }

        private static bool IsValidSetting(Type type)
        {
            if (type == null) return false;
            if (!type.IsClass) return false;
            if (type.IsAbstract) return false;
            if (type.IsGenericType) return false;
            if (type.ContainsGenericParameters) return false;

            return ImplementsSettingInterface(type);
        }

        private static bool ImplementsSettingInterface(Type type)
        {
            return typeof(ISetting).IsAssignableFrom(type);
        }
    }
}