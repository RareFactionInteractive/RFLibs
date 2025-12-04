using System;
using RFLibs.DependencyInjection;
using UnitTests.MVVM.Models;

namespace UnitTests.MVVM.Spells
{
    public class SpellModel
    {
        public enum SpellEffectType
        {
            Heal,
            Damage,
            //Buff,
            //Debuff,
            None
        }

        public string Name { get; private set; }
        public int ManaCost { get; private set; }
        public float CooldownSeconds { get; private set; }
        public SpellEffectType EffectType { get; private set; }
        public int EffectMagnitude { get; private set; }

        private long _lastCastTimeTicks = -1;

        protected SpellModel(string name, int manaCost, float cooldownSeconds, SpellEffectType effectType, int effectMagnitude)
        {
            Name = name;
            ManaCost = manaCost;
            CooldownSeconds = cooldownSeconds;
            EffectType = effectType;
            EffectMagnitude = effectMagnitude;

            DI.InjectDependencies(this);
        }

        public bool IsOffCooldown(long ticks) => ticks >= _lastCastTimeTicks + TimeSpan.FromSeconds(CooldownSeconds).Ticks;

        public void MarkCast(long ticks) => _lastCastTimeTicks = ticks;
    }

    public class DamageSpell : SpellModel
    {
        public DamageSpell(string name, int manaCost, float cooldownSeconds, int damage)
            : base(name, manaCost, cooldownSeconds, SpellEffectType.Damage, damage)
        {
        }
    }

    public class HealSpell : SpellModel
    {
        public HealSpell(string name, int manaCost, float cooldownSeconds, int healAmount)
            : base(name, manaCost, cooldownSeconds, SpellEffectType.Heal, healAmount)
        {
        }
    }
}
