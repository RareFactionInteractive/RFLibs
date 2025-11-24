using RFLibs.Core;

namespace RFLibs.DI
{
    public static class DI
    {
        private static DIContainer? _globalContainer;
        private static DIContainer? _sceneContainer;


        public static DIContainer InitializeGlobal(DIContainer container)
        {
            _globalContainer = container;
            return container;
        }

        public static DIContainer InitializeScene(DIContainer container)
        {
            _sceneContainer = container;
            return container;
        }

        internal static DIContainer Container =>
            _sceneContainer ?? _globalContainer ?? throw new System.InvalidOperationException("DI not initialized");
        
        public static Result<bool, DIErrors> Bind<TInterface>(object implementation)
        {
            return Container.Bind<TInterface>(implementation);
        }

        public static Result<T, DIErrors> Resolve<T>()
        {
            return Container.Resolve<T>();
        }

        public static void InjectDependencies(object instance)
        {
            Container.InjectDependencies(instance);
        }

        public static void Clear()
        {
            Container?.Clear();
        }
    }
}