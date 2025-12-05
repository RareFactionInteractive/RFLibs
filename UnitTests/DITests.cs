#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
using System;
using NUnit.Framework;
using RFLibs.DependencyInjection;
using RFLibs.DependencyInjection.Attributes;

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
            public string LastMessage { get; private set; } = string.Empty;
            public void Log(string message) => LastMessage = message;
        }

        private class DummyTestApplication
        {
            [Inject] public ITestService? _testService;
            public bool Test => _testService!.PerformTest();

            public new int GetHashCode() => _testService!.GetHashCode();
        }

        private class ComplexApplication
        {
            [Inject] private ITestService? _testService;
            [Inject] private ICalculatorService? _calculatorService;
            [Inject] private ILoggerService? _loggerService;

            public bool TestAll()
            {
                var result = _testService!.PerformTest();
                Console.WriteLine($"Test result: {result}");
                var calculation = _calculatorService!.Add(2, 3);
                Console.WriteLine($"Calculation: {calculation}");
                _loggerService!.Log($"Test result: {result}, Calculation: {calculation}");
                return result && calculation == 5;
            }

            public ILoggerService GetLogger() => _loggerService!;
        }

        private class ServiceWithDependency
        {
            [Inject] private ILoggerService? _logger;

            public void DoWork()
            {
                _logger!.Log("Work completed");
            }

            public ILoggerService GetLogger() => _logger!;
        }

        private class ApplicationWithPropertyInjection
        {
            [Inject] public ITestService? TestService { get; set; }
            [Inject] public ICalculatorService? CalculatorService { get; set; }

            public bool RunTests()
            {
                var testResult = TestService!.PerformTest();
                var calcResult = CalculatorService!.Add(10, 20);
                return testResult && calcResult == 30;
            }
        }

        private class ApplicationWithMixedInjection
        {
            [Inject] private ITestService? _testServiceField;
            [Inject] public ICalculatorService? CalculatorProperty { get; set; }

            public bool RunMixedTest()
            {
                var testResult = _testServiceField!.PerformTest();
                var calcResult = CalculatorProperty!.Add(5, 7);
                return testResult && calcResult == 12;
            }

            public ITestService GetFieldService() => _testServiceField!;
            public ICalculatorService GetPropertyService() => CalculatorProperty!;
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
                DI.Resolve<ITestService>(out var result1);
                Assert.That(app1.GetHashCode(), Is.EqualTo(result1.Ok?.GetHashCode()));
                DI.Resolve<ITestService>(out var result2);
                Assert.That(app2.GetHashCode(), Is.EqualTo(result2.Ok?.GetHashCode()));
            });
        }

        [Test, Order(6)]
        public void DICanResolve()
        {
            var testService = new DummyTestService();
            DI.Bind<ITestService>(testService);

            bool success = DI.Resolve<ITestService>(out var resolved);

            Assert.Multiple(() =>
            {
                Assert.That(success, Is.True);
                Assert.That(resolved.Ok, Is.SameAs(testService));
            });
        }

        [Test, Order(7)]
        public void DIReturnsErrorWhenResolvingUnboundType()
        {
            DI.Clear();
            bool success = DI.Resolve<ITestService>(out _);
            Assert.That(success, Is.False);
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

            bool beforeClear = DI.Resolve<ITestService>(out _);
            Assert.That(beforeClear, Is.True);
            DI.Clear();
            bool afterClear = DI.Resolve<ITestService>(out _);
            Assert.That(afterClear, Is.False);
        }

        [Test, Order(12)]
        public void TransientLifetimeCreatesNewInstancesOnEachResolve()
        {
            // CalculatorService is marked with [Lifetime(Lifetime.Transient)]
            DI.Bind<ICalculatorService>(new CalculatorService());

            bool success1 = DI.Resolve<ICalculatorService>(out var instance1);
            bool success2 = DI.Resolve<ICalculatorService>(out var instance2);

            Assert.Multiple(() =>
            {
                Assert.That(success1, Is.True, "First resolve should succeed");
                Assert.That(success2, Is.True, "Second resolve should succeed");
                Assert.That(Object.ReferenceEquals(instance1.Ok, instance2.Ok), Is.False, 
                    "Transient should create new instances, not reuse the same one");
            });
        }

        [Test, Order(13)]
        public void DICanInjectIntoFields()
        {
            // Bind required services
            DI.Bind<ITestService>(new DummyTestService());
            DI.Bind<ICalculatorService>(new CalculatorService());
            DI.Bind<ILoggerService>(new LoggerService());

            // Create application with field injection
            var app = new ComplexApplication();
            DI.InjectDependencies(app);

            // Verify field injection worked
            Assert.That(app.TestAll(), Is.True, "Field injection should successfully inject all dependencies");
        }

        [Test, Order(14)]
        public void DICanInjectIntoProperties()
        {
            // Bind required services
            DI.Bind<ITestService>(new DummyTestService());
            DI.Bind<ICalculatorService>(new CalculatorService());

            // Create application with property injection
            var app = new ApplicationWithPropertyInjection();
            DI.InjectDependencies(app);

            // Verify property injection worked
            Assert.Multiple(() =>
            {
                Assert.That(app.TestService, Is.Not.Null, "TestService property should be injected");
                Assert.That(app.CalculatorService, Is.Not.Null, "CalculatorService property should be injected");
                Assert.That(app.RunTests(), Is.True, "Property injection should successfully inject all dependencies");
            });
        }

        [Test, Order(15)]
        public void DICanInjectIntoMixedFieldsAndProperties()
        {
            // Bind required services
            var testService = new DummyTestService();
            var calcService = new CalculatorService();
            DI.Bind<ITestService>(testService);
            DI.Bind<ICalculatorService>(calcService);

            // Create application with both field and property injection
            var app = new ApplicationWithMixedInjection();
            DI.InjectDependencies(app);

            // Verify both field and property injection worked
            Assert.Multiple(() =>
            {
                Assert.That(app.GetFieldService(), Is.SameAs(testService), 
                    "Field injection should inject the correct instance");
                Assert.That(app.GetPropertyService(), Is.Not.Null, 
                    "Property injection should inject a service instance");
                Assert.That(app.RunMixedTest(), Is.True, 
                    "Mixed injection should successfully inject all dependencies");
            });
        }

        [Test, Order(16)]
        public void DICanUnbindAndRebindSingleton()
        {
            // Bind a singleton instance
            var originalService = new DummyTestService();
            DI.Bind<ITestService>(originalService);

            bool success1 = DI.Resolve<ITestService>(out var resolved1);
            Assert.That(resolved1.Ok, Is.SameAs(originalService), "Should resolve to the original instance");

            // Try to bind again without unbinding - should return the existing singleton
            var newService = new DummyTestService();
            var bindResult = DI.Bind<ITestService>(newService);
            Assert.That(bindResult.Ok, Is.SameAs(originalService), "Binding again should return the existing singleton");

            // Unbind the singleton
            var unbindResult = DI.Unbind<ITestService>();
            Assert.That(unbindResult, Is.True, "Unbind should succeed");

            // After unbinding, resolve should fail
            bool successAfterUnbind = DI.Resolve<ITestService>(out _);
            Assert.That(successAfterUnbind, Is.False, "Should not resolve after unbinding");

            // Rebind with new instance
            DI.Bind<ITestService>(newService);
            bool success2 = DI.Resolve<ITestService>(out var resolved2);
            Assert.Multiple(() =>
            {
                Assert.That(resolved2.Ok, Is.SameAs(newService), "Should resolve to the new instance after rebinding");
                Assert.That(resolved2.Ok, Is.Not.SameAs(originalService), "Should not be the original instance");
            });
        }

        [TearDown]
        public void TearDown()
        {
            DI.Clear();
        }
    }
}
#pragma warning restore CS8618