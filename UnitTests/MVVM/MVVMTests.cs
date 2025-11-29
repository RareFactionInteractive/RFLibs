using NUnit.Framework;
using RFLibs.DependencyInjection;

namespace UnitTests.MVVM
{
    public class MvvmTests
    {
        private readonly EnemyController _goblinController = new(new Goblin());
        private readonly EnemyController _orcController = new(new Orc());
        private DummyLabelBinder _dummyLabelBinder;

        [SetUp]
        public void Setup()
        {
           DI.Bind(new PlayerModel(100));
           DI.Bind(new PlayerViewModel());
           _dummyLabelBinder = new();
        }

        [Test, Order(0)]
        public void TestPlayerModel()
        {
            Assert.Multiple(() =>
            {
                Assert.That(DI.Resolve<PlayerViewModel>().Ok.Health.Value, Is.EqualTo(100));
                Assert.That(_dummyLabelBinder.Text, Is.EqualTo("Health: 100"));
            });
        }

        [Test, Order(1)]
        public void ModelTakesDamageWhenControllerAttacks()
        {
            _goblinController.Attack(DI.Resolve<PlayerViewModel>().Ok);
            Assert.That(DI.Resolve<PlayerViewModel>().Ok.Health.Value, Is.EqualTo(90));

            _orcController.Attack(DI.Resolve<PlayerViewModel>().Ok);
            Assert.That(DI.Resolve<PlayerViewModel>().Ok.Health.Value, Is.EqualTo(70));
        }

        [Test, Order(2)]
        public void ModelHealsWhenViewModelHeals()
        {
            DI.Resolve<PlayerViewModel>().Ok.Heal(30);
            Assert.That(DI.Resolve<PlayerViewModel>().Ok.Health.Value, Is.EqualTo(100));
        }
    }
}