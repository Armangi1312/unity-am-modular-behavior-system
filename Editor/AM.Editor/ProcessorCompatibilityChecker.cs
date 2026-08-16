using AM.Core;
using System;

namespace AM.Editor
{
    internal sealed class ProcessorCompatibilityChecker
    {
        private readonly Type settingType;
        private readonly Type contextType;
        private readonly Type processorType;

        public bool IsValid => settingType != null 
                            && contextType != null 
                            && processorType != null;

        public ProcessorCompatibilityChecker(Type settingType, Type contextType, Type processorType)
        {
            this.settingType = settingType;
            this.contextType = contextType;
            this.processorType = processorType;
        }

        public bool IsCandidate(Type type) => type != null 
                                           && !type.IsAbstract 
                                           && !type.IsInterface 
                                           && !type.ContainsGenericParameters;

        public bool IsCompatible(Type candidate)
        {
            if (!IsValid) return false;
            if (!IsCandidate(candidate)) return false;
            if (!processorType.IsAssignableFrom(candidate)) return false;

            var interfaces = candidate.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                var @interface = interfaces[i];
                if (!@interface.IsGenericType) continue;
                if (@interface.GetGenericTypeDefinition() != typeof(IProcessor<,>)) continue;

                var args = @interface.GetGenericArguments();
                if (settingType.IsAssignableFrom(args[0]) &&
                    contextType.IsAssignableFrom(args[1]))
                    return true;
            }

            return false;
        }
    }
}
