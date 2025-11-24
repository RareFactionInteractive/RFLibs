using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RFLibs.Core;

namespace RFLibs.DI
{
    public class DIContainer
    {
        private readonly Dictionary<Type, object> _instances = new();

        public Result<bool, DIErrors> Bind<TInterface>(object implementation)
        {
            if (implementation is not TInterface)
            {
                return Result<bool, DIErrors>.Error(DIErrors.InvalidType);
            }

            _instances[typeof(TInterface)] = implementation;   
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
                var dependency = Resolve(field.FieldType);
                field.SetValue(instance, dependency.Ok);
            }
        }

        public void Clear()
        {
            _instances.Clear();
        }
    }
}