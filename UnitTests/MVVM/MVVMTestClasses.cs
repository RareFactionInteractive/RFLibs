using UnitTests.MVVM.Models;
using UnitTests.MVVM.ViewModels;
using UnitTests.MVVM.Spells;

// This file serves as a central re-export point for MVVM test classes
// The actual implementations are organized in subdirectories:
// - Models: Domain models (PlayerModel, EnemyModel, etc.)
// - ViewModels: ViewModel classes (PlayerViewModel, SpellViewModel)
// - Spells: Spell definitions (SpellModel, DamageSpell, HealSpell)
// - Mocks: Test mock/helper classes (DummyLabelBinder, MockSpellButton, MockLabel)

namespace UnitTests.MVVM
{
    // Re-export the TimeProvider delegate for convenience
    public delegate long TimeProvider();
}
