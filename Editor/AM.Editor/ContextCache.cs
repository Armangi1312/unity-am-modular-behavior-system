using AM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace AM.Editor
{
    [InitializeOnLoad]
    internal static class ContextCache
    {
        public static readonly IReadOnlyList<Type> ContextTypes;

        private static readonly Dictionary<Type, List<Type>> filterCache = new();

        private static readonly object cacheLock = new();

        private static readonly List<Type> emptyList = new(0);

        static ContextCache()
        {
            ContextTypes = FindContextTypes();

            AssemblyReloadEvents.afterAssemblyReload += () =>
            {
                lock (cacheLock)
                {
                    filterCache.Clear();
                }
            };
        }

        public static List<Type> GetFilteredContextTypes(Type targetType)
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

            for (int i = 0; i < ContextTypes.Count; i++)
            {
                var ContextType = ContextTypes[i];

                if (!isGenericTarget && targetType.IsAssignableFrom(ContextType))
                {
                    result.Add(ContextType);
                    continue;
                }

                foreach (var iface in ContextType.GetInterfaces())
                {
                    if (!iface.IsGenericType)
                        continue;

                    if (iface.GetGenericTypeDefinition() == targetType)
                    {
                        result.Add(ContextType);
                        break;
                    }
                }
            }

            return result;
        }

        private static List<Type> FindContextTypes()
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
                    if (!IsValidContext(type))
                        continue;

                    result.Add(type);
                }
            }

            result.Sort((a, b) =>
                string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            return result;
        }

        private static bool IsValidContext(Type type)
        {
            if (type == null) return false;
            if (!type.IsClass) return false;
            if (type.IsAbstract) return false;
            if (type.IsGenericType) return false;
            if (type.ContainsGenericParameters) return false;

            return ImplementsContextInterface(type);
        }

        private static bool ImplementsContextInterface(Type type)
        {
            return type.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IContext)
            );
        }
    }
}