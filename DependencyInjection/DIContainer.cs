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

        public Result<bool, DIErrors> Bind<TInterface>(TInterface implementation, Lifetime lifetime)
        {
            if (implementation == null)
            {
                return Result<bool, DIErrors>.Error(DIErrors.NullBinding);
            }

            _registrations[typeof(TInterface)] = new RegistrationInfo
            {
                Instance = implementation,
                Lifetime = lifetime,
                ConcreteType = implementation.GetType()
            };
            
            return Result<bool, DIErrors>.OK(true);
        }

        public Result<T, DIErrors> Resolve<T>()
        {
            var result = Resolve(typeof(T));
            return result.IsOk ? 
                Result<T, DIErrors>.OK((T)result.Ok) :
                Result<T, DIErrors>.Error(DIErrors.CannotResolve);
        }

        private Result<object, bool> Resolve(Type type)
        {
            if (!_registrations.TryGetValue(type, out var registration))
            {
                return Result<object, bool>.Error(false);
            }

            // For Singleton, return the cached instance
            if (registration.Lifetime == Lifetime.Singleton)
            {
                return Result<object, bool>.OK(registration.Instance);
            }

            // For Transient, create a new instance
            try
            {
                var newInstance = Activator.CreateInstance(registration.ConcreteType);
                return Result<object, bool>.OK(newInstance);
            }
            catch
            {
                return Result<object, bool>.Error(false);
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