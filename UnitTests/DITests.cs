using System;
using NUnit.Framework;
using RFLibs.DI;

namespace UnitTests
{
    public class DITests
    {
        private interface ITestService
        {
            bool PerformTest();
        }

        private interface ICalculatorService
        {
            int Add(int a, int b);
        }

        private interface ILoggerService
        {
            void Log(string message);
        }

        [Lifetime(Lifetime.Singleton)]
        private class DummyTestService : ITestService
        {
            public bool PerformTest()
            {
                return true;
            }
        }

        [Lifetime(Lifetime.Transient)]
        private class CalculatorService : ICalculatorService
        {
            public int Add(int a, int b) => a + b;
        }

        [Scope(Scope.Scene)]
        private class LoggerService : ILoggerService
        {
            public string LastMessage { get; private set; }
            public void Log(string message) => LastMessage = message;
        }

        private class DummyTestApplication
        {
            [Inject] public ITestService _testService;
            public bool Test => _testService.PerformTest();

            public new int GetHashCode() => _testService.GetHashCode();
        }

        private class ComplexApplication
        {
            [Inject] private ITestService _testService;
            [Inject] private ICalculatorService _calculatorService;
            [Inject] private ILoggerService _loggerService;

            public bool TestAll()
            {
                var result = _testService.PerformTest();
                Console.WriteLine($"Test result: {result}");
                var calculation = _calculatorService.Add(2, 3);
                Console.WriteLine($"Calculation: {calculation}");
                _loggerService.Log($"Test result: {result}, Calculation: {calculation}");
                return result && calculation == 5;
            }

            public ILoggerService GetLogger() => _loggerService;
        }

        private class ServiceWithDependency
        {
            [Inject] private ILoggerService _logger;

            public void DoWork()
            {
                _logger.Log("Work completed");
            }

            public ILoggerService GetLogger() => _logger;
        }

        [Test, Order(0)]
        public void DICanInjectDependencies()
        {
            DI.Bind<ITestService>(new DummyTestService());

            var testApplication = new DummyTestApplication();
            DI.InjectDependencies(testApplication);

            Assert.That(testApplication.Test, Is.True);
        }

        [Test, Order(1)]
        public void DICanBindMultipleServices()
        {
            Assert.Multiple(() =>
            {
                Assert.That(DI.Bind<ITestService>(new DummyTestService()).IsOk);
                Assert.That(DI.Bind<ICalculatorService>(new CalculatorService()).IsOk);
                Assert.That(DI.Bind<ILoggerService>(new LoggerService()).IsOk);
            });

            var app = new ComplexApplication();
            DI.InjectDependencies(app);

            Assert.That(app.TestAll(), Is.True);
            
            Assert.Multiple(() =>
            {
                Assert.That(((LoggerService)app.GetLogger()).LastMessage,
                    Is.EqualTo("Test result: True, Calculation: 5"));
            });
        }

        [Test, Order(3)]
        public void DIContainerHandlesSceneScope()
        {
            var sceneLogger = new LoggerService();
            DI.Bind<ILoggerService>(sceneLogger);

            var service = new ServiceWithDependency();
            DI.InjectDependencies(service);
            service.DoWork();

            Assert.Multiple(() =>
            {
                // Should use the scene container's logger
                Assert.That(service.GetLogger(), Is.SameAs(sceneLogger));
                Assert.That(sceneLogger.LastMessage, Is.EqualTo("Work completed"));
            });
        }

        [Test, Order(5)]
        public void DIHandlesSingletonScope()
        {
            // DummyTestService is marked with [ServiceScope(ServiceScope.Singleton)]
            DI.Bind<ITestService>(new DummyTestService());

            var app1 = new DummyTestApplication();
            var app2 = new DummyTestApplication();

            DI.InjectDependencies(app1);
            DI.InjectDependencies(app2);

            Assert.Multiple(() =>
            {
                Assert.That(app1.GetHashCode(), Is.EqualTo(DI.Resolve<ITestService>().Ok.GetHashCode()));
                Assert.That(app2.GetHashCode(), Is.EqualTo(DI.Resolve<ITestService>().Ok.GetHashCode()));
            });
        }

        [Test, Order(6)]
        public void DICanResolve()
        {
            var testService = new DummyTestService();
            DI.Bind<ITestService>(testService);

            var resolved = DI.Resolve<ITestService>();

            Assert.Multiple(() =>
            {
                Assert.That(resolved.IsOk);
                Assert.That(resolved.Ok, Is.SameAs(testService));
            });
        }

        [Test, Order(7)]
        public void DIReturnsErrorWhenResolvingUnboundType()
        {
            DI.Clear();
            Assert.That(DI.Resolve<ITestService>().IsErr);
        }

        [Test, Order(8)]
        public void DIContainerCanInjectIntoObjectsWithNoInjectableFields()
        {
            var objectWithoutDependencies = new object();

            // Should not throw an exception when injecting into objects with no [Inject] fields
            Assert.DoesNotThrow(() =>
                DI.InjectDependencies(objectWithoutDependencies));
        }

        [Test, Order(11)]
        public void DIContainerClearRemovesAllBindings()
        {
            DI.Bind<ITestService>(new DummyTestService());

            Assert.That(DI.Resolve<ITestService>().IsOk);
            DI.Clear();
            Assert.That(DI.Resolve<ITestService>().IsErr);
        }

        [Test, Order(12)]
        public void TransientLifetimeCreatesNewInstancesOnEachResolve()
        {
            // CalculatorService is marked with [Lifetime(Lifetime.Transient)]
            DI.Bind<ICalculatorService>(new CalculatorService());

            var instance1 = DI.Resolve<ICalculatorService>();
            var instance2 = DI.Resolve<ICalculatorService>();

            Assert.Multiple(() =>
            {
                Assert.That(instance1.IsOk, "First resolve should succeed");
                Assert.That(instance2.IsOk, "Second resolve should succeed");
                Assert.That(Object.ReferenceEquals(instance1.Ok, instance2.Ok), Is.False, 
                    "Transient should create new instances, not reuse the same one");
            });
        }

        [TearDown]
        public void TearDown()
        {
            DI.Clear();
        }
    }
}