using RFLibs.DependencyInjection.Attributes;

namespace UnitTests.MVVM.Models
{
    [Scope(Scope.Global)]
    [Lifetime(Lifetime.Singleton)]
    public class PlayerModel : CharacterModel, ITargetable
    {
        public PlayerModel(string name, int maxHealth, int attackPower, int maxMana) 
            : base(name, maxHealth, attackPower, maxMana) { }
    }
}
