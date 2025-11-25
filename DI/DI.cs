using System;
using System.Linq;
using System.Reflection;
using RFLibs.Core;

namespace RFLibs.DI
{
    public static class DI
    {
        private static DIContainer _globalContainer;
        private static DIContainer _sceneContainer;


        private static void InitializeGlobal()
        {
            _globalContainer = new DIContainer();
        }

        private static void InitializeScene()
        {
            _sceneContainer = new DIContainer();
        }

        internal static DIContainer Container
        {
            get
            {
                if(_sceneContainer != null)
                {
                    return _sceneContainer;
                }
                if(_globalContainer != null)
                {
                    return _globalContainer;
                }
                
                InitializeGlobal();
                return _globalContainer;
            }
        }
        
        public static Result<bool, DIErrors> Bind<TInterface>(TInterface implementation)
        {
            if (implementation == null)
            {
                return Result<bool, DIErrors>.Error(DIErrors.NullBinding);
            }

            var implementationType = implementation.GetType();
            
            // Read Lifetime attribute (default: Singleton)
            var lifetimeAttribute = (LifetimeAttribute)Attribute.GetCustomAttribute(
                implementationType, 
                typeof(LifetimeAttribute)
            );
            var lifetime = lifetimeAttribute?.Lifetime ?? Lifetime.Singleton;

            // Read Scope attribute (default: Global)
            var scopeAttribute = (ScopeAttribute)Attribute.GetCustomAttribute(
                implementationType, 
                typeof(ScopeAttribute)
            );
            var scope = scopeAttribute?.Scope ?? Scope.Global;

            // Route to appropriate container based on scope
            switch (scope)
            {
                case Scope.Global:
                    // Bind to global container
                    if (_globalContainer == null)
                    {
                        InitializeGlobal();
                    }
                    return _globalContainer.Bind(implementation, lifetime);

                case Scope.Scene:
                    // Bind to scene container
                    if (_sceneContainer == null)
                    {
                        InitializeScene();
                    }
                    return _sceneContainer.Bind(implementation, lifetime);

                default:
                    return Result<bool, DIErrors>.Error(DIErrors.NullBinding);
            }
        }

        public static Result<T, DIErrors> Resolve<T>()
        {
            // Try global container first
            if (_globalContainer != null)
            {
                var result = _globalContainer.Resolve<T>();
                if (result.IsOk)
                {
                    return result;
                }
            }

            // Fall back to scene container
            if (_sceneContainer != null)
            {
                var result = _sceneContainer.Resolve<T>();
                if (result.IsOk)
                {
                    return result;
                }
            }

            return Result<T, DIErrors>.Error(DIErrors.CannotResolve);
        }

        public static void InjectDependencies(object instance)
        {
            var fields = instance.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.GetCustomAttribute<InjectAttribute>() != null);

            foreach (var field in fields)
            {
                // Use DI.Resolve which checks both containers
                var resolveMethod = typeof(DI).GetMethod(nameof(Resolve), BindingFlags.Public | BindingFlags.Static);
                var genericResolve = resolveMethod.MakeGenericMethod(field.FieldType);
                var result = genericResolve.Invoke(null, null);
                
                var resultType = result.GetType();
                var isOkProperty = resultType.GetProperty("IsOk");
                var okProperty = resultType.GetProperty("Ok");
                
                if ((bool)isOkProperty.GetValue(result))
                {
                    field.SetValue(instance, okProperty.GetValue(result));
                }
            }
        }

        public static void Clear()
        {
            if(_globalContainer != null)
            {
                _globalContainer.Clear();
                _globalContainer = null;
            }
            if(_sceneContainer != null)
            {
                _sceneContainer.Clear();
                _sceneContainer = null;
            }
        }
    }
}