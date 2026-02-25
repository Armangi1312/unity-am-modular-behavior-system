using AM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace AM.Editor
{
    [InitializeOnLoad]
    internal static class ProcessorCache
    {
        public static readonly List<Type> ProcessorTypes;

        static ProcessorCache()
        {
            ProcessorTypes = GetProcessorTypes();
        }

        private static List<Type> GetProcessorTypes()
        {
            var result = new List<Type>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                var asm = assemblies[i];

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

                for (int j = 0; j < types.Length; j++)
                {
                    var t = types[j];

                    if (t == null)
                        continue;

                    if (t.IsAbstract)
                        continue;

                    if (t.IsGenericType)
                        continue;

                    if (!typeof(Processor).IsAssignableFrom(t))
                        continue;

                    result.Add(t);
                }
            }

            result.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            return result;
        }
    }
}