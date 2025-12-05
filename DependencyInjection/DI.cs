using System;
using System.Linq;
using System.Reflection;
using RFLibs.Core;
using RFLibs.DependencyInjection.Attributes;

namespace RFLibs.DependencyInjection
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
        
        public static Result<TInterface, DIErrors> Bind<TInterface>(TInterface implementation)
        {
            if (implementation == null)
            {
                return Result<TInterface, DIErrors>.Error(DIErrors.NullBinding);
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
                    return Result<TInterface, DIErrors>.Error(DIErrors.NullBinding);
            }
        }

        /// <summary>
        /// Unbinds a service from the appropriate container (Global or Scene).
        /// For Singletons, this allows rebinding with a new instance.
        /// </summary>
        public static bool Unbind<TInterface>()
        {
            // Try scene container first
            if (_sceneContainer != null && _sceneContainer.Unbind<TInterface>())
            {
                return true;
            }

            // Fall back to global container
            if (_globalContainer != null && _globalContainer.Unbind<TInterface>())
            {
                return true;
            }

            return false;
        }

        public static bool Resolve<T>(out Result<T, DIErrors> result)
        {
            // Try global container first
            if (_globalContainer != null)
            {
                if(_globalContainer.Resolve<T>(out result))
                if (result.IsOk)
                {
                    return true;
                }
            }

            // Fall back to scene container
            if (_sceneContainer != null)
            {
                if(_sceneContainer.Resolve<T>(out result))
                if (result.IsOk)
                {
                    return true;
                }
            }

            result = Result<T, DIErrors>.Error(DIErrors.CannotResolve);
            return false;
        }

        public static void InjectDependencies(object instance)
        {
            // Inject into fields
            var fields = instance.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.GetCustomAttribute<InjectAttribute>() != null);

            foreach (var field in fields)
            {
                // Use DI.Resolve which checks both containers
                var resolveMethod = typeof(DI).GetMethod(nameof(Resolve), BindingFlags.Public | BindingFlags.Static);
                var genericResolve = resolveMethod.MakeGenericMethod(field.FieldType);
                
                // Create an out parameter for the result
                object?[] parameters = new object?[1];
                bool success = (bool)genericResolve.Invoke(null, parameters);
                
                if (success)
                {
                    var result = parameters[0];
                    var resultType = result.GetType();
                    var okProperty = resultType.GetProperty("Ok");
                    field.SetValue(instance, okProperty.GetValue(result));
                }
            }

            // Inject into properties
            var properties = instance.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(p => p.GetCustomAttribute<InjectAttribute>() != null && p.CanWrite);

            foreach (var property in properties)
            {
                // Use DI.Resolve which checks both containers
                var resolveMethod = typeof(DI).GetMethod(nameof(Resolve), BindingFlags.Public | BindingFlags.Static);
                var genericResolve = resolveMethod.MakeGenericMethod(property.PropertyType);
                
                // Create an out parameter for the result
                object?[] parameters = new object?[1];
                bool success = (bool)genericResolve.Invoke(null, parameters);
                
                if (success)
                {
                    var result = parameters[0];
                    var resultType = result.GetType();
                    var okProperty = resultType.GetProperty("Ok");
                    property.SetValue(instance, okProperty.GetValue(result));
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

        /// <summary>
        /// Clears only the scene container, preserving global services.
        /// Call this when unloading a scene to clean up scene-scoped services.
        /// </summary>
        public static void ClearSceneContainer()
        {
            if (_sceneContainer != null)
            {
                _sceneContainer.Clear();
                _sceneContainer = null;
            }
        }
    }
}