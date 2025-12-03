# Spell Targeting System

This document demonstrates how to target an `EnemyModel` with a `Spell` as the player.

## Overview

The spell targeting system allows players to cast spells on specific targets (enemies, allies, or themselves). This is implemented using:
- Generic `ICommand<T>` and `Command<T>` for parameterized commands
- `ITargetable` interface that both `PlayerModel` and `EnemyModel` implement
- `SpellViewModel.CastOnTargetCommand` for executing targeted spells

## Key Components

### ITargetable Interface
Both players and enemies implement this interface, making them valid spell targets:

```csharp
public interface ITargetable : IHealable, IDamageable { }
```

### EnemyModel
Enemies now have Health and can be targeted:

```csharp
public abstract class EnemyModel : ITargetable
{
    public Bindable<int> Health { get; protected set; }
    public int MaxHealth { get; protected set; }
    
    public void TakeDamage(int amount) { /* ... */ }
    public void Heal(int amount) { /* ... */ }
    public bool IsDead => Health.Value <= 0;
}
```

### SpellViewModel
The spell view model provides two ways to cast spells:

1. **CastCommand**: Casts on the current target (or self if no target is set)
2. **CastOnTargetCommand**: Casts on a specific target passed as parameter

```csharp
public class SpellViewModel
{
    public ICommand CastCommand { get; }
    public ICommand<ITargetable> CastOnTargetCommand { get; }
    
    public void SetTarget(ITargetable target) { /* ... */ }
}
```

## Targeting Summary

The spell system supports three targeting modes:

| Method | Command | Use Case | Example |
|--------|---------|----------|---------|
| **Explicit Targeting** | `CastOnTargetCommand.Execute(target)` | Direct control over target | Cast fireball on a specific enemy |
| **Default Target** | `SetTarget(target)` then `CastCommand.Execute()` | Tab-targeting systems | Set enemy as target, then cast |
| **Self-Cast** | `CastCommand.Execute()` (no target set) | Player buffs/heals | Cast heal without setting a target |

## Usage Examples

### Example 1: Casting a Damage Spell on an Enemy

```csharp
// Create an enemy
var goblin = new Goblin(); // 50 HP

// Create a damage spell
var fireballModel = new DamageSpell("Fireball", manaCost: 20, cooldownSeconds: 1.0f, damage: 15);
var fireballViewModel = new SpellViewModel(fireballModel);

// Cast the spell on the goblin
fireballViewModel.CastOnTargetCommand.Execute(goblin);

// Result: Goblin now has 35 HP
```

### Example 2: Healing an Enemy

```csharp
// Create a heal spell
var healModel = new HealSpell("Minor Heal", manaCost: 10, cooldownSeconds: 0.5f, healAmount: 10);
var healViewModel = new SpellViewModel(healModel);

// Heal the goblin
healViewModel.CastOnTargetCommand.Execute(goblin);

// Result: Goblin health increases by 10
```

### Example 3: Using SetTarget for Default Casting

```csharp
var orc = new Orc();
var spellViewModel = new SpellViewModel(fireballModel);

// Set the orc as the default target
spellViewModel.SetTarget(orc);

// Cast without specifying target (uses the set target)
spellViewModel.CastCommand.Execute();

// Result: Orc takes damage
```

### Example 4: Player Self-Targeting

The player can target themselves in two ways:

**Method 1: Explicit Self-Targeting**
```csharp
var player = DI.Resolve<PlayerModel>().Ok;
var healViewModel = new SpellViewModel(healModel);

// Explicitly target the player
healViewModel.CastOnTargetCommand.Execute(player);

// Result: Player heals themselves
```

**Method 2: Implicit Self-Cast (Recommended for self-buffs)**
```csharp
var healViewModel = new SpellViewModel(healModel);

// Don't set a target - spell automatically targets the player
healViewModel.CastCommand.Execute();

// Result: Player heals themselves (no target needed)
```

Both methods work identically, but Method 2 is more convenient for spells that are commonly self-cast (like shields, buffs, or heals).

## Creating Custom Spells

To create a custom spell, inherit from `SpellModel`:

```csharp
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
```

## Spell Effects

The system currently supports these effect types:
- **Heal**: Increases target's health (capped at MaxHealth)
- **Damage**: Decreases target's health (minimum 0)
- **Buff**: (Not yet implemented)
- **Debuff**: (Not yet implemented)

## Validation

The targeting system includes validation:
- Checks if player has enough mana
- Checks if spell is off cooldown
- For targeted spells, checks if target is not null

```csharp
private bool CanCastOnTarget(ITargetable target)
{
    return target != null &&
           _player.Mana.Value >= _model.ManaCost &&
           _model.IsOffCooldown(_timeProvider());
}
```

## Integration with Unity

In a Unity context, you might use this system like:

```csharp
// In your UI controller
public void OnSpellButtonClick()
{
    if (selectedEnemy != null)
    {
        spellViewModel.CastOnTargetCommand.Execute(selectedEnemy);
    }
}

// Or for click-to-target gameplay
public void OnEnemyClicked(EnemyModel enemy)
{
    activeSpellViewModel.CastOnTargetCommand.Execute(enemy);
}
```

## Testing

See `MVVMTests.cs` for a complete test example:

```csharp
[Test]
public void PlayerCanTargetEnemyWithSpell()
{
    var goblin = new Goblin();
    var fireballViewModel = new SpellViewModel(fireballModel);
    
    fireballViewModel.CastOnTargetCommand.Execute(goblin);
    
    Assert.That(goblin.Health.Value, Is.EqualTo(35));
}
```
