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

        public static Result<T, DIErrors> Resolve<T>(object[] constructorArgs = null)
        {
            var typeToResolve = typeof(T);
            
            // Read Scope attribute (default: Global)
            var scopeAttribute = (ScopeAttribute)Attribute.GetCustomAttribute(
                typeToResolve, 
                typeof(ScopeAttribute)
            );
            var scope = scopeAttribute?.Scope ?? Scope.Global;

            // Try the primary container first based on scope
            switch (scope)
            {
                case Scope.Global:
                    if (_globalContainer == null)
                    {
                        InitializeGlobal();
                    }
                    if (_globalContainer.Resolve<T>(out var globalResult, constructorArgs))
                    {
                        return globalResult;
                    }
                    // Fall through to check scene container as fallback
                    if (_sceneContainer != null && _sceneContainer.Resolve<T>(out var fallbackResult, constructorArgs))
                    {
                        return fallbackResult;
                    }
                    break;

                case Scope.Scene:
                    if (_sceneContainer == null)
                    {
                        InitializeScene();
                    }
                    if (_sceneContainer.Resolve<T>(out var sceneResult, constructorArgs))
                    {
                        return sceneResult;
                    }
                    // Fall through to check global container as fallback
                    if (_globalContainer != null && _globalContainer.Resolve<T>(out var fallbackResult2, constructorArgs))
                    {
                        return fallbackResult2;
                    }
                    break;
            }

            return Result<T, DIErrors>.Error(DIErrors.CannotResolve);
        }

        /// <summary>
        /// Convenience method to resolve a service and get the instance directly.
        /// Throws an exception if resolution fails. Use this for fail-fast behavior and method chaining.
        /// </summary>
        public static T ResolveOrThrow<T>(object[] constructorParams = null)
        {
            var result = Resolve<T>(constructorParams ?? Array.Empty<object>());
            if (result.IsErr)
            {
                throw new InvalidOperationException($"Failed to resolve {typeof(T).Name}: {result.Err}");
            }
            return result.Ok;
        }

        public static T ResolveOrDefault<T>(object[] constructorParams = null)
        {
            var result = Resolve<T>(constructorParams ?? Array.Empty<object>());
            return result.IsOk ? result.Ok : default(T);
        }

        // Delegate for generic field injection
        private static Delegate CreateFieldInjector(Type fieldType)
        {
            try
            {
                var method = typeof(DI).GetMethod("GenericInjectField", BindingFlags.NonPublic | BindingFlags.Static);
                if (method == null)
                {
                    throw new InvalidOperationException($"GenericInjectField method not found");
                }
                var generic = method.MakeGenericMethod(fieldType);
                return Delegate.CreateDelegate(
                    typeof(Action<object, FieldInfo>),
                    generic
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateFieldInjector error: {ex}");
                throw;
            }
        }

        // Delegate for generic property injection
        private static Delegate CreatePropertyInjector(Type propertyType)
        {
            try
            {
                var method = typeof(DI).GetMethod("GenericInjectProperty", BindingFlags.NonPublic | BindingFlags.Static);
                if (method == null)
                {
                    throw new InvalidOperationException($"GenericInjectProperty method not found");
                }
                var generic = method.MakeGenericMethod(propertyType);
                return Delegate.CreateDelegate(
                    typeof(Action<object, PropertyInfo>),
                    generic
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreatePropertyInjector error: {ex}");
                throw;
            }
        }

        private static void GenericInjectField<T>(object instance, FieldInfo field)
        {
            var result = Resolve<T>();
            if (result.IsOk)
            {
                field.SetValue(instance, result.Ok);
            }
        }

        private static void GenericInjectProperty<T>(object instance, PropertyInfo property)
        {
            var result = Resolve<T>();
            if (result.IsOk)
            {
                property.SetValue(instance, result.Ok);
            }
        }

        public static void InjectDependencies(object instance)
        {
            // Inject into fields
            var fields = instance.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.GetCustomAttribute<InjectAttribute>() != null);

            foreach (var field in fields)
            {
                var injector = CreateFieldInjector(field.FieldType);
                ((Action<object, FieldInfo>)injector)(instance, field);
            }

            // Inject into properties
            var properties = instance.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(p => p.GetCustomAttribute<InjectAttribute>() != null && p.CanWrite);

            foreach (var property in properties)
            {
                var injector = CreatePropertyInjector(property.PropertyType);
                ((Action<object, PropertyInfo>)injector)(instance, property);
            }
        }

        public static void Clear()
        {
            _globalContainer?.Clear();
            _globalContainer = null;
            _sceneContainer?.Clear();
            _sceneContainer = null;
        }

        /// <summary>
        /// Clears only the scene container, preserving global services.
        /// Call this when unloading a scene to clean up scene-scoped services.
        /// </summary>
        public static void ClearSceneContainer()
        {
            _sceneContainer?.Clear();
            _sceneContainer = null;
        }
    }
}