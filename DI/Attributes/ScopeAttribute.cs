using System;

namespace RFLibs.DI
{
    /// <summary>
    /// Defines when service registrations are cleared.
    /// </summary>
    public enum Scope
    {
        /// <summary>
        /// Registration persists for the entire application lifetime.
        /// </summary>
        Global,
        
        /// <summary>
        /// Registration is cleared when the scene is unloaded.
        /// </summary>
        Scene
    }

    /// <summary>
    /// Specifies the scope for a service registration.
    /// Default is Global if not specified.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ScopeAttribute : Attribute
    {
        public Scope Scope { get; }

        public ScopeAttribute(Scope scope)
        {
            Scope = scope;
        }
    }
}
