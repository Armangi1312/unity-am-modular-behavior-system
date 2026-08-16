using AM.Core;
using System;
using UnityEngine;

namespace AM.Editor
{
    internal static class ControllerTypeResolver
    {
        public static bool TryResolve(Controller controller, out Type settingType, out Type contextType, out Type processorType)
        {
            var type = controller.GetType();

            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Controller<,,>))
                {
                    var args = type.GetGenericArguments();

                    settingType = args[0];
                    contextType = args[1];
                    processorType = args[2];

                    return true;
                }
                type = type.BaseType;
            }

            Debug.LogWarning("[ControllerTypeResolver] Could not resolve generic type arguments.");

            settingType = null;
            contextType = null;
            processorType = null;

            return false;
        }
    }
}
