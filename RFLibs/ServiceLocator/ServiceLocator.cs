using System;
using System.Collections.Generic;

namespace RFLibs.ServiceLocator
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> _SERVICE_CONTAINER = new();
        private static readonly Dictionary<Type, List<Action<IService>>> _BOUND_CALLBACKS = new();
        //private static readonly Dictionary<System.Type, List<System.Action>> _UNBOUND_CALLBACKS = new();

        public static T Bind<T>(T service) where T : IService
        {
            if (_SERVICE_CONTAINER.TryGetValue(typeof(T), out var existingService))
            {
                return (T)existingService;
            }

            _SERVICE_CONTAINER[typeof(T)] = service;

            if (_BOUND_CALLBACKS.TryGetValue(typeof(T), out var callbacks))
            {
                callbacks.ForEach(callback => callback(service));
            }

            return service;
        }

        public static bool TryGet<T>(out T? service) where T : class, IService
        {
            service = default;
            if (!_SERVICE_CONTAINER.TryGetValue(typeof(T), out var existingService))
            {
                return false;
            }

            service = (T?)existingService;
            return true;
        }

        public static bool Unbind<T>() where T : IService
        {
            return _SERVICE_CONTAINER.Remove(typeof(T));
        }

        public static void WhenBound<T>(Action<T> callback) where T : IService
        {
            if (_SERVICE_CONTAINER.TryGetValue(typeof(T), out var existingService))
            {
                callback((T)existingService);
                return;
            }

            if (!_BOUND_CALLBACKS.TryGetValue(typeof(T), out var callbacks))
            {
                callbacks = new List<Action<IService>>();
                _BOUND_CALLBACKS[typeof(T)] = callbacks;
            }

            callbacks.Add(service => callback((T)service));
        }
    }
}