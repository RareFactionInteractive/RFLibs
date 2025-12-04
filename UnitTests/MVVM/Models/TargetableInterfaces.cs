namespace UnitTests.MVVM.Models
{
    public interface IHealable { void Heal(int amount); }
    public interface IDamageable { void TakeDamage(int amount); }
    public interface ITargetable : IHealable, IDamageable { }
}
