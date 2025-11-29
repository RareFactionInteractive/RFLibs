using System;

namespace RFLibs.DependencyInjection.Attributes
{
    /// <summary>
    /// Defines how instances are created when resolving dependencies.
    /// </summary>
    public enum Lifetime
    {
        /// <summary>
        /// A single instance is created and reused for all resolutions.
        /// </summary>
        Singleton,
        
        /// <summary>
        /// A new instance is created for each resolution.
        /// </summary>
        Transient
    }

    /// <summary>
    /// Specifies the lifetime pattern for a service.
    /// Default is Singleton if not specified.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LifetimeAttribute : Attribute
    {
        public Lifetime Lifetime { get; }

        public LifetimeAttribute(Lifetime lifetime)
        {
            Lifetime = lifetime;
        }
    }
}
