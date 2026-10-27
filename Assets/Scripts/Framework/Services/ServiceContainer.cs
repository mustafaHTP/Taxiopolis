using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Services
{
    public class ServiceContainer
    {
        private readonly Dictionary<Type, object> _services = new();
        public IEnumerable<object> Services => _services.Values;

        public void RegisterService<T>(T service)
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"Service of type {type} is already registered. Overwriting the existing service.");
            }
            _services[type] = service;
        }

        public void RegisterService(Type type, object service)
        {
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"Service of type {type} is already registered. Overwriting the existing service.");
            }
            _services[type] = service;
        }

        public T GetService<T>()
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException($"Service of type {type} is not registered.");
        }

        public bool TryGetService<T>(out T service)
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var registeredService))
            {
                service = (T)registeredService;
                return true;
            }

            service = default;

            return false;
        }

        public void UnregisterService<T>()
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
            }
            else
            {
                Debug.LogWarning($"Service of type {type} is not registered. Cannot unregister.");
            }
        }

        public void ClearServices()
        {
            _services.Clear();
        }
    }
}
