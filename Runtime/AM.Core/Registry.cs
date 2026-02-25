using System;
using System.Collections.Generic;
using UnityEngine;

namespace AM.Core
{
    [Serializable]
    public sealed class Registry<TTarget> : IRegistry<TTarget>, ISerializationCallbackReceiver where TTarget : class
    {
        [SerializeReference]
        private List<TTarget> serializedObjects = new();

        public List<TTarget> SerializedObjects
        {
            get
            {
                serializedObjects ??= new();
                return serializedObjects;
            }
        }

        private readonly Dictionary<Type, TTarget> registries = new();

        public void Register<T>(T instance) where T : class, TTarget
        {
            Register(typeof(T), instance);
        }

        public void Register(Type type, object instance)
        {
            if (instance is not TTarget target)
                throw new ArgumentException($"Invalid instance for {type.Name}");

            registries[type] = target;

            int index = SerializedObjects.FindIndex(o => o?.GetType() == type);

            if (index >= 0)
                SerializedObjects[index] = target;
            else
                SerializedObjects.Add(target);
        }

        public T Get<T>() where T : class, TTarget
        {
            if (registries.TryGetValue(typeof(T), out var value))
                return (T)value;

            throw new Exception($"{typeof(T).Name} is not registered.");
        }

        public object Get(Type type)
        {
            if (registries.TryGetValue(type, out var value))
                return value;

            throw new Exception($"{type.Name} is not registered.");
        }

        public bool TryGet<T>(out T value) where T : class, TTarget
        {
            if (registries.TryGetValue(typeof(T), out var ctx))
            {
                value = (T)ctx;
                return true;
            }

            value = null;
            return false;
        }

        public bool Contains(Type type) => registries.ContainsKey(type);

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            registries.Clear();

            if (serializedObjects == null) return;

            foreach (var obj in serializedObjects)
            {
                if (obj != null)
                    registries[obj.GetType()] = obj;
            }
        }
    }
}