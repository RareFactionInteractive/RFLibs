namespace RFLibs.DI
{
    public static class DIContext
    {
        public static DIContainer GlobalContainer { get; private set; }
        public static DIContainer SceneContainer { get; private set; }

        public static void InitializeGlobal(DIContainer container)
        {
            GlobalContainer = container;
        }

        public static void InitializeScene(DIContainer container)
        {
            SceneContainer = container;
        }

        public static DIContainer Container =>
            SceneContainer ?? GlobalContainer;
    }
}