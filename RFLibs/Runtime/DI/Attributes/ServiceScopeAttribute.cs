using System;

namespace RFLibs.DI
{
    public enum ServiceScope
    {
        Singleton,
        Scene,
        Transient
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceScopeAttribute : Attribute
    {
        public ServiceScope Scope { get; }

        public ServiceScopeAttribute(ServiceScope scope)
        {
            Scope = scope;
        }
    }
}