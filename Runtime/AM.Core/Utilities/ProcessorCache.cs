using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace AM.Core.Utilities
{
    [InitializeOnLoad]
    public static class ProcessorCache
    {
        public static readonly IReadOnlyList<Type> ProcessorTypes;

        private static readonly Dictionary<Type, List<Type>> filterCache = new();
        private static readonly object cacheLock = new();

        private static readonly List<Type> emptyList = new(0);

        static ProcessorCache()
        {
            ProcessorTypes = FindProcessorTypes();

            AssemblyReloadEvents.afterAssemblyReload += () =>
            {
                lock (cacheLock)
                {
                    filterCache.Clear();
                }
            };
        }

        public static List<Type> GetFilteredProcessorTypes(Type targetType)
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

            for (int i = 0; i < ProcessorTypes.Count; i++)
            {
                var processorType = ProcessorTypes[i];

                // 일반 상속 검사
                if (!isGenericTarget && targetType.IsAssignableFrom(processorType))
                {
                    result.Add(processorType);
                    continue;
                }

                // Generic Processor<T1,T2> 대응
                var baseType = processorType.BaseType;

                while (baseType != null)
                {
                    if (baseType.IsGenericType &&
                        baseType.GetGenericTypeDefinition() == targetType)
                    {
                        result.Add(processorType);
                        break;
                    }

                    baseType = baseType.BaseType;
                }
            }

            return result;
        }

        private static List<Type> FindProcessorTypes()
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
                    if (!IsValidProcessor(type))
                        continue;

                    result.Add(type);
                }
            }

            result.Sort((a, b) =>
                string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            return result;
        }

        private static bool IsValidProcessor(Type type)
        {
            if (type == null) return false;
            if (!type.IsClass) return false;
            if (type.IsAbstract) return false;
            if (type.IsGenericType) return false;
            if (type.ContainsGenericParameters) return false;

            return IsProcessorType(type);
        }

        private static bool IsProcessorType(Type type)
        {
            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IProcessor<,>));
        }
    }
}