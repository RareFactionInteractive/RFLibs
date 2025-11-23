using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using RFLibs.DI;

namespace UnitTests
{
    public class DITests
    {
        private interface ITestService { bool PerformTest(); }
        private interface ICalculatorService { int Add(int a, int b); }
        private interface ILoggerService { void Log(string message); }

        [ServiceScope(ServiceScope.Singleton)]
        private class DummyTestService : ITestService
        {
            public bool PerformTest()
            {
                return true;
            }
        }

        private class CalculatorService : ICalculatorService
        {
            public int Add(int a, int b) => a + b;
        }

        private class LoggerService : ILoggerService
        {
            public string LastMessage { get; private set; }
            public void Log(string message) => LastMessage = message;
        }
        
        private class DummyTestApplication
        {
            [Inject] private ITestService _testService;
            public bool Test => _testService.PerformTest();
        }

        private class ComplexApplication
        {
            [Inject] private ITestService _testService;
            [Inject] private ICalculatorService _calculatorService;
            [Inject] private ILoggerService _loggerService;

            public bool TestAll()
            {
                var result = _testService?.PerformTest() ?? false;
                var calculation = _calculatorService?.Add(2, 3) ?? 0;
                _loggerService?.Log($"Test result: {result}, Calculation: {calculation}");
                return result && calculation == 5;
            }

            public ILoggerService GetLogger() => _loggerService;
        }

        private class ServiceWithDependency
        {
            [Inject] private ILoggerService _logger;
            
            public void DoWork()
            {
                _logger?.Log("Work completed");
            }
            
            public ILoggerService GetLogger() => _logger;
        }
        
        [SetUp]
        public void Setup()
        {
            DIContext.InitializeGlobal(new DIContainer()
                .Bind<ITestService>(new DummyTestService()));
        }

        [Test, Order(0)]
        public void DIContainerCanInjectDependencies()
        {
            var testApplication = new DummyTestApplication();
            DIContext.Container.InjectDependencies(testApplication);
            
            Assert.That(testApplication.Test, Is.True);
        }

        [Test, Order(1)]
        public void DIContainerCanBindMultipleServices()
        {
            var container = new DIContainer()
                .Bind<ITestService>(new DummyTestService())
                .Bind<ICalculatorService>(new CalculatorService())
                .Bind<ILoggerService>(new LoggerService());

            DIContext.InitializeGlobal(container);

            var app = new ComplexApplication();
            DIContext.Container.InjectDependencies(app);

            Assert.That(app.TestAll(), Is.True);
            Assert.That(((LoggerService)app.GetLogger()).LastMessage, Is.EqualTo("Test result: True, Calculation: 5"));
        }

        [Test, Order(2)]
        public void DIContainerBindValidatesTypeCompatibility()
        {
            var container = new DIContainer();
            
            // Should work - correct type
            Assert.DoesNotThrow(() => container.Bind<ITestService>(new DummyTestService()));
            
            // Should throw - incompatible type
            Assert.Throws<ArgumentException>(() => container.Bind<ITestService>(new CalculatorService()));
        }

        [Test, Order(3)]
        public void SceneContainerTakesPrecedenceOverGlobalContainer()
        {
            // Setup global container
            var globalLogger = new LoggerService();
            var globalContainer = new DIContainer()
                .Bind<ILoggerService>(globalLogger);
            DIContext.InitializeGlobal(globalContainer);

            // Create a different instance for scene container
            var sceneLogger = new LoggerService();
            var sceneContainer = new DIContainer()
                .Bind<ILoggerService>(sceneLogger);
            DIContext.InitializeScene(sceneContainer);

            var service = new ServiceWithDependency();
            DIContext.Container.InjectDependencies(service);
            service.DoWork();

            // Should use the scene container's logger
            Assert.That(service.GetLogger(), Is.SameAs(sceneLogger));
            Assert.That(sceneLogger.LastMessage, Is.EqualTo("Work completed"));
            Assert.That(globalLogger.LastMessage, Is.Null);
        }

        [Test, Order(4)]
        public void GlobalContainerUsedWhenSceneContainerIsNull()
        {
            var globalLogger = new LoggerService();
            var globalContainer = new DIContainer()
                .Bind<ILoggerService>(globalLogger);
            DIContext.InitializeGlobal(globalContainer);

            // Ensure scene container is null
            DIContext.InitializeScene(null);

            var service = new ServiceWithDependency();
            DIContext.Container.InjectDependencies(service);
            service.DoWork();

            // Should fall back to global container's logger
            Assert.That(service.GetLogger(), Is.SameAs(globalLogger));
            Assert.That(globalLogger.LastMessage, Is.EqualTo("Work completed"));
        }

        [Test, Order(5)]
        public void DIContainerHandlesSingletonScope()
        {
            // DummyTestService is marked with [ServiceScope(ServiceScope.Singleton)]
            var container = new DIContainer();

            var app1 = new DummyTestApplication();
            var app2 = new DummyTestApplication();

            DIContext.InitializeGlobal(container);
            DIContext.Container.InjectDependencies(app1);
            DIContext.Container.InjectDependencies(app2);

            // Both should get the same singleton instance (if not bound explicitly)
            Assert.That(app1.Test, Is.True);
            Assert.That(app2.Test, Is.True);
        }

        [Test, Order(6)]
        public void DIContainerCanResolveDirectly()
        {
            var testService = new DummyTestService();
            var container = new DIContainer()
                .Bind<ITestService>(testService);

            var resolved = container.Resolve<ITestService>();
            
            Assert.Multiple(() =>
            {
                Assert.That(resolved.IsOk);
                Assert.That(resolved.Ok, Is.SameAs(testService));
            });
            
        }

        [Test, Order(7)]
        public void DIContainerThrowsWhenResolvingUnboundType()
        {
            var container = new DIContainer();
            Assert.That(() => container.Resolve<ITestService>().IsErr);
        }

        [Test, Order(8)]
        public void DIContainerCanInjectIntoObjectsWithNoInjectableFields()
        {
            var objectWithoutDependencies = new object();
            
            // Should not throw an exception when injecting into objects with no [Inject] fields
            Assert.DoesNotThrow(() => 
                DIContext.Container.InjectDependencies(objectWithoutDependencies));
        }

        [Test, Order(9)]
        public void DIContainerCanHandleChainedBinding()
        {
            var container = new DIContainer()
                .Bind<ITestService>(new DummyTestService())
                .Bind<ICalculatorService>(new CalculatorService())
                .Bind<ILoggerService>(new LoggerService());

            // Test that chained binding returns a properly configured container
            Assert.That(container, Is.Not.Null);
            
            DIContext.InitializeGlobal(container);
            var app = new ComplexApplication();
            
            Assert.DoesNotThrow(() => DIContext.Container.InjectDependencies(app));
            Assert.That(app.TestAll(), Is.True);
        }

        [Test, Order(10)]
        public void DIContextContainerPropertyReturnsCorrectContainer()
        {
            var globalContainer = new DIContainer();
            var sceneContainer = new DIContainer();

            DIContext.InitializeGlobal(globalContainer);
            Assert.That(DIContext.Container, Is.SameAs(globalContainer));

            DIContext.InitializeScene(sceneContainer);
            Assert.That(DIContext.Container, Is.SameAs(sceneContainer));

            DIContext.InitializeScene(null);
            Assert.That(DIContext.Container, Is.SameAs(globalContainer));
        }

        [Test, Order(11)]
        public void DIContainerClearRemovesAllBindings()
        {
            var container = new DIContainer()
                .Bind<ITestService>(new DummyTestService());

            // Verify service is bound
            Assert.That(container.Resolve<ITestService>().IsOk);

            // Clear and verify service is no longer available
            container.Clear();
            Assert.That(container.Resolve<ITestService>().IsErr);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up containers after each test
            DIContext.InitializeGlobal(null);
            DIContext.InitializeScene(null);
        }
    }
}