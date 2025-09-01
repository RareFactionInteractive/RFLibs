namespace Runtime.ServiceLocator;

public static class ServiceLocator
{
    private static readonly Dictionary<System.Type, IService> _SERVICE_CONTAINER = new();
    //private static readonly Dictionary<System.Type, List<System.Action<IService>>> _BOUND_CALLBACKS = new();
    //private static readonly Dictionary<System.Type, List<System.Action>> _UNBOUND_CALLBACKS = new();

    public static T Bind<T>(T service) where T : IService
    {
        if(_SERVICE_CONTAINER.TryGetValue(typeof(T), out var existingService))
        {
            return (T)existingService;
        }
        
        _SERVICE_CONTAINER[typeof(T)] = service;
        return service;
    }

    public static bool TryGet<T>(out T? service) where T : IService
    {
        service = default;
        if (!_SERVICE_CONTAINER.TryGetValue(typeof(T), out var existingService))
        {
            return false;
        }
        
        service = (T)existingService;
        return true;
    }
    
    public static bool Unbind<T>() where T : IService
    {
        return _SERVICE_CONTAINER.Remove(typeof(T));
    }
}