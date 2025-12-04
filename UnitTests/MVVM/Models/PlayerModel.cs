using System;
using RFLibs.MVVM;
using RFLibs.DependencyInjection.Attributes;

namespace UnitTests.MVVM.Models
{
    [Scope(Scope.Global)]
    [Lifetime(Lifetime.Singleton)]
    public class PlayerModel : ITargetable
    {
        public Bindable<int> Health { get; private set; }
        public Bindable<int> Mana { get; private set; }

        public PlayerModel(int health, int mana = 0)
        {
            Health = new Bindable<int>(health);
            Mana = new Bindable<int>(mana);
        }

        public void TakeDamage(int amount)
        {
            Health.Value = Math.Max(0, Health.Value - amount);
        }

        public void Heal(int amount)
        {
            Health.Value = Math.Min(100, Health.Value + amount);
        }

        public bool SpendMana(int amount)
        {
            if (Mana.Value < amount) return false;
            Mana.Value -= amount;
            return true;
        }

        public void AddMana(int amount)
        {
            Mana.Value = Math.Min(100, Mana.Value + amount);
        }
    }
}
