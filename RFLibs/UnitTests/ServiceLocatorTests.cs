using Runtime.ServiceLocator;

namespace UnitTests
{
    public class ServiceLocatorTests
    {
        private interface IDummyService : IService;

        private class DummyService : IDummyService;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ServiceLocatorCanBind()
        {
            var dummyService = ServiceLocator.Bind<IDummyService>(new DummyService());
            Assert.That(dummyService, Is.Not.Null);
        }

        [Test]
        public void ServiceLocatorCanGet()
        {
            var result = ServiceLocator.TryGet<IDummyService>(out var dummyService);
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(dummyService, Is.Not.Null);
                Assert.That(dummyService, Is.TypeOf<DummyService>());
            });
        }
        
        [Test]
        public void ServiceLocatorCanUnbind()
        {
            var result = ServiceLocator.Unbind<IDummyService>();
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(ServiceLocator.TryGet<IDummyService>(out var dummyService), Is.False);
                Assert.That(dummyService, Is.Null);
            });
        }
    }
}