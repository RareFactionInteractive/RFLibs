using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RFLibs.Core;
using RFLibs.DependencyInjection.Attributes;

namespace RFLibs.DependencyInjection

{
    internal class DIContainer
    {
        private class RegistrationInfo
        {
            public object Instance { get; set; }
            public Lifetime Lifetime { get; set; }
            public Type ConcreteType { get; set; }
        }

        private readonly Dictionary<Type, RegistrationInfo> _registrations = new();

        public Result<TInterface, DIErrors> Bind<TInterface>(TInterface implementation, Lifetime lifetime)
        {
            if (implementation == null)
            {
                return Result<TInterface, DIErrors>.Error(DIErrors.NullBinding);
            }

            var interfaceType = typeof(TInterface);
            
            // Check if already bound as a Singleton
            if (_registrations.TryGetValue(interfaceType, out var existingRegistration) &&
                existingRegistration.Lifetime == Lifetime.Singleton)
            {
                // Return the existing singleton instance instead of rebinding
                return Result<TInterface, DIErrors>.OK((TInterface)existingRegistration.Instance);
            }

            _registrations[interfaceType] = new RegistrationInfo
            {
                Instance = implementation,
                Lifetime = lifetime,
                ConcreteType = implementation.GetType()
            };
            
            return Result<TInterface, DIErrors>.OK(implementation);
        }

        public bool Unbind<TInterface>()
        {
            return _registrations.Remove(typeof(TInterface));
        }

        public bool Resolve<T>(out Result<T, DIErrors> result, object[] constructorArgs = null)
        {
            var val = Resolve(typeof(T), constructorArgs);
            result = val.IsOk ? 
                Result<T, DIErrors>.OK((T)val.Ok) :
                Result<T, DIErrors>.Error(DIErrors.CannotResolve);

            return result.IsOk;
        }

        private Result<object, DIErrors> Resolve(Type type, object[] constructorArgs = null)
        {
            if (!_registrations.TryGetValue(type, out var registration))
            {
                return Result<object, DIErrors>.Error(DIErrors.CannotResolve);
            }

            // For Singleton, return the cached instance
            if (registration.Lifetime == Lifetime.Singleton)
            {
                return Result<object, DIErrors>.OK(registration.Instance);
            }

            // For Transient, create a new instance
            try
            {
                var newInstance = Activator.CreateInstance(registration.ConcreteType, constructorArgs);
                return Result<object, DIErrors>.OK(newInstance);
            }
            catch
            {
                return Result<object, DIErrors>.Error(DIErrors.TransientClassInstantiationFailed);
            }
        }

        public void InjectDependencies(object instance)
        {
            var fields = instance.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.GetCustomAttribute<InjectAttribute>() != null);

            foreach (var field in fields)
            {
                var dependency = Resolve(field.FieldType);
                field.SetValue(instance, dependency.Ok);
            }
        }

        public void Clear()
        {
            _registrations.Clear();
        }
    }
}