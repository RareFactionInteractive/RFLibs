using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RFLibs.Core.BEGiN.Core;

namespace RFLibs.DI
{
    public class DIContainer
    {
        private readonly Dictionary<Type, object> _instances = new();
        private readonly ServiceScope _defaultScope;

        public DIContainer(ServiceScope defaultScope = ServiceScope.Singleton)
        {
            _defaultScope = defaultScope;
        }

        public DIContainer Bind<TInterface>(object implementation)
        {
            if(implementation is not TInterface) throw new ArgumentException($"Cannot bind {implementation.GetType()} to {typeof(TInterface)}");
            _instances[typeof(TInterface)] = implementation;   
            return this;
        }

        public DIContainer Bind<TInterface, TImplementation>()
        {
            
            return this;
        }

        public Result<T, bool> Resolve<T>()
        {
            var result = Resolve(typeof(T));
            return result.IsOk ? 
                Result<T, bool>.OK((T)result.Ok) :
                Result<T, bool>.Error(false);
        }

        private Result<object, bool> Resolve(Type type)
        {
            return _instances.TryGetValue(type, out var existing) 
                ? Result<object, bool>.OK(existing) :
                Result<object, bool>.Error(false);
        }

        public void InjectDependencies(object instance)
        {
            var fields = instance.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.GetCustomAttribute<InjectAttribute>() != null);

            foreach (var field in fields)
            {
                var dependency = Resolve(field.FieldType).Ok;
                field.SetValue(instance, dependency);
            }
        }

        public void Clear()
        {
            _instances.Clear();
        }
    }
}