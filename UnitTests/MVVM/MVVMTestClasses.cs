using System;
using RFLibs.MVVM;
using RFLibs.DependencyInjection.Attributes;
using RFLibs.DependencyInjection;

namespace UnitTests.MVVM
{
    [Scope(Scope.Global)]
    [Lifetime(Lifetime.Singleton)]
    public class PlayerModel
    {
        public Bindable<int> Health { get; private set; }

        public PlayerModel(int health)
        {
            Health = new Bindable<int>(health);
        }
    }

    [Scope(Scope.Global)]
    [Lifetime(Lifetime.Singleton)]
    public class PlayerViewModel
    {
        [Inject] private readonly PlayerModel _model;
        public Bindable<int> Health => _model.Health;

        public PlayerViewModel()
        {
            DI.InjectDependencies(this);
        }

        public void TakeDamage(int amount)
        {
            Health.Value = Math.Max(0, Health.Value - amount);
        }

        public void Heal(int amount)
        {
            Health.Value = Math.Min(100, Health.Value + amount);
        }
    }

    public abstract class EnemyModel
    {
        public string Name { get; protected set; }
        public int Damage { get; protected set; }

        protected EnemyModel(string name, int damage)
        {
            Name = name;
            Damage = damage;
        }
    }

    public class Goblin : EnemyModel
    {
        public Goblin() : base("Goblin", 10) { }
    }

    public class Orc : EnemyModel
    {
        public Orc() : base("Orc", 20) { }
    }

    public class EnemyController
    {
        private readonly EnemyModel _model;

        public EnemyController(EnemyModel model)
        {
            _model = model;
        }

        public void Attack(PlayerViewModel player)
        {
            player.TakeDamage(_model.Damage);
        }
    }

    public class DummyLabelBinder : Binder<PlayerViewModel>
    {
        public string Text { get; private set; }

        public DummyLabelBinder()
        {
            //We are outside of Unity, so OnEnable is never called.
            base.OnEnable();
            UpdateHealth(ViewModel.Health.Value);
        }

        protected override void OnBind()
        {
            ViewModel.Health.OnValueChanged += UpdateHealth;
        }

        private void UpdateHealth(int health)
        {
            Text = $"Health: {health}";
        }   

        ~DummyLabelBinder()
        {
            ViewModel.Health.OnValueChanged -= UpdateHealth;
        }   
    }
}