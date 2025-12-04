using System;
using RFLibs.MVVM;

namespace UnitTests.MVVM.Models
{
    public abstract class EnemyModel : ITargetable
    {
        public string Name { get; protected set; }
        public int Damage { get; protected set; }
        public Bindable<int> Health { get; protected set; }
        public int MaxHealth { get; protected set; }

        protected EnemyModel(string name, int damage, int maxHealth)
        {
            Name = name;
            Damage = damage;
            MaxHealth = maxHealth;
            Health = new Bindable<int>(maxHealth);
        }

        public void TakeDamage(int amount)
        {
            Health.Value = Math.Max(0, Health.Value - amount);
        }

        public void Heal(int amount)
        {
            Health.Value = Math.Min(MaxHealth, Health.Value + amount);
        }

        public bool IsDead => Health.Value <= 0;
    }

    public class Goblin : EnemyModel
    {
        public Goblin() : base("Goblin", 10, 50) { }
    }

    public class Orc : EnemyModel
    {
        public Orc() : base("Orc", 20, 100) { }
    }

    public class EnemyController
    {
        private readonly EnemyModel _model;

        public EnemyController(EnemyModel model)
        {
            _model = model;
        }

        public void Attack(IDamageable target)
        {
            target.TakeDamage(_model.Damage);
        }
    }
}
