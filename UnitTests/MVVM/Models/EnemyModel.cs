namespace UnitTests.MVVM.Models
{
    public abstract class EnemyModel : CharacterModel, ITargetable
    {
        protected EnemyModel(string name, int maxHealth, int attackPower, int maxMana = 0)
            : base(name, maxHealth, attackPower, maxMana) { }
    }

    public class Goblin : EnemyModel
    {
        public Goblin() : base("Goblin", 50, 10) { }
    }

    public class Orc : EnemyModel
    {
        public Orc() : base("Orc", 20, 20, 10) { }
    }
}
