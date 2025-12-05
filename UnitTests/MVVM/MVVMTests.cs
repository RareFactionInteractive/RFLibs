using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RFLibs.DependencyInjection;
using UnitTests.MVVM.Models;
using UnitTests.MVVM.ViewModels;
using UnitTests.MVVM.Spells;
using UnitTests.MVVM.Mocks;
using NUnit.Framework.Interfaces;

namespace UnitTests.MVVM
{
    public class MvvmTests
    {
        private DummyLabelBinder _dummyLabelBinder;
        private PlayerModel _playerModel;
        private PlayerViewModel _playerVM;

        [OneTimeSetUp]
        public void Setup()
        {
            DI.Bind<TimeProvider>(() => DateTime.Now.Ticks);
            DI.Bind(new PlayerModel("Player", 100, 10, 100));
            DI.Bind(new PlayerViewModel());

            _dummyLabelBinder = new();
        }

        [SetUp]
        public void ResetPlayerForEachTest()
        {
            // Reset player stats before each test
            Assert.That(DI.Resolve<PlayerModel>(out var playerResult), Is.True, "PlayerModel should resolve");
            _playerModel = playerResult.Ok;

            Assert.That(DI.Resolve<PlayerViewModel>(out var playerVMResult), Is.True, "PlayerViewModel should resolve");
            _playerVM = playerVMResult.Ok;
            
            var player = playerResult.Ok;
            player.Health = 100;
            player.Mana = 100;
        }

        [Test, Order(0)]
        public void TestPlayerModel()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_playerModel.Health, Is.EqualTo(100));
                Assert.That(_dummyLabelBinder.Text, Is.EqualTo("Health: 100"));
            });
        }

        [Test, Order(1)]
        public void ModelTakesDamageWhenEnemiesAttack()
        {
            var goblin = new Goblin();
            var orc = new Orc();

            goblin.Attack(_playerModel);
            Assert.Multiple(() =>
            {
                Assert.That(_playerModel.Health, Is.EqualTo(90));
                Assert.That(_dummyLabelBinder.Text, Is.EqualTo("Health: 90"));
            });

            orc.Attack(_playerModel);
            Assert.Multiple(() =>
            {
                Assert.That(_playerModel.Health, Is.EqualTo(70));
                Assert.That(_dummyLabelBinder.Text, Is.EqualTo("Health: 70"));
            });
        }

        [Test, Order(2)]
        public void PlayerCanTargetEnemyWithSpell()
        {
            var goblin = new Goblin();

            // Create a damage spell
            var fireballModel = new DamageSpell("Fireball", manaCost: 20, cooldownSeconds: 1.0f, damage: 15);
            var fireballViewModel = new SpellViewModel(fireballModel);

            // Player casts fireball on goblin
            Assert.That(goblin.Health, Is.EqualTo(goblin.MaxHealth), "Goblin should start with Max Health");
            fireballViewModel.CastOnTargetCommand.Execute(goblin);

            Assert.Multiple(() =>
            {
                Assert.That(goblin.Health, Is.EqualTo(Math.Max(0, goblin.MaxHealth - fireballModel.EffectMagnitude)), "Goblin should have taken 15 damage");
                Assert.That(_playerModel.Mana, Is.EqualTo(80), "Player should have spent 20 mana");
            });

            // Create a heal spell and test healing the enemy
            var healModel = new HealSpell("Minor Heal", manaCost: 10, cooldownSeconds: 0.5f, healAmount: 10);
            var healViewModel = new SpellViewModel(healModel);

            var goblinHealthBeforeHeal = goblin.Health;
            healViewModel.CastOnTargetCommand.Execute(goblin);
            Assert.Multiple(() =>
            {
                Assert.That(goblin.Health, Is.EqualTo(goblinHealthBeforeHeal + healModel.EffectMagnitude), "Goblin should have 45 HP after healing");
                Assert.That(_playerModel.Mana, Is.EqualTo(70), "Player should have spent another 10 mana");
            });
        }

        [Test, Order(3)]
        public async Task PlayerCanTargetThemselfWithSpell()
        {
            // Create a heal spell
            var healModel = new HealSpell("Minor Heal", manaCost: 15, cooldownSeconds: 0.5f, healAmount: 20);
            var healViewModel = new SpellViewModel(healModel);

            // Method 1: Explicitly target the player
            healViewModel.CastOnTargetCommand.Execute(_playerModel);

            Assert.Multiple(() =>
            {
                Assert.That(_playerModel.Health, Is.EqualTo(100), "Player should stay at 100 HP (already at max after healing 20)");
                Assert.That(_playerModel.Mana, Is.EqualTo(85), "Player should have spent 15 mana");
            });

            await Task.Delay((int)(healModel.CooldownSeconds * 1000));

            // Method 2: Self-cast (no target set defaults to player)
            healViewModel.CastCommand.Execute();

            Assert.Multiple(() =>
            {
                Assert.That(_playerModel.Health, Is.EqualTo(100), "Player should stay at 100 HP (already at max)");
                Assert.That(_playerModel.Mana, Is.EqualTo(70), "Player should have spent another 15 mana");
            });

            await Task.Delay((int)(healModel.CooldownSeconds * 1000));

            // Verify healing doesn't exceed max health
            healViewModel.CastCommand.Execute();
            Assert.That(_playerModel.Health, Is.EqualTo(100), "Player health should cap at 100");
        }

        [Test, Order(4)]
        public void MockButtonDemonstratesTwoWayBindingWithSpellCasting()
        {
            
            var goblin = new Goblin();

            // Create a heal spell for self-healing
            var healModel = new HealSpell("Heal", manaCost: 20, cooldownSeconds: 2.0f, healAmount: 30);
            var healViewModel = new SpellViewModel(healModel);

            // Create a damage spell for attacking
            var fireballModel = new DamageSpell("Fireball", manaCost: 15, cooldownSeconds: 1.5f, damage: 25);
            var fireballViewModel = new SpellViewModel(fireballModel);

            // Create mock buttons that bind to the commands
            var healButton = new MockSpellButton(healViewModel.CastCommand);
            var fireballButton = new MockSpellButton(fireballViewModel.CastOnTargetCommand, goblin);

            // Create UI elements that bind to player stats
            var healthLabel = new MockLabel();
            healthLabel.BindToValue(_playerVM.Health, "Health");
            var manaLabel = new MockLabel();
            manaLabel.BindToValue(_playerVM.Mana, "Mana");

            // Note: Goblin doesn't have a ViewModel, so we can't bind its raw health property
            // For now, we'll just check values directly in assertions
            // var goblinHealthLabel = new MockLabel();
            // goblinHealthLabel.BindToValue(goblin.Health, "Health");

            Console.WriteLine("=== Mock Button Two-Way Binding Demo ===\n");

            // Initial state
            Console.WriteLine("Initial State:");
            Console.WriteLine($"  {healthLabel.Text}");
            Console.WriteLine($"  {manaLabel.Text}");
            Console.WriteLine($"  Goblin Health: {goblin.Health}");
            Console.WriteLine($"  Heal Button Enabled: {healButton.IsEnabled}");
            Console.WriteLine($"  Fireball Button Enabled: {fireballButton.IsEnabled}\n");

            Assert.Multiple(() =>
            {
                Assert.That(healthLabel.Text, Is.EqualTo("Health: 100"));
                Assert.That(manaLabel.Text, Is.EqualTo("Mana: 100"));
                Assert.That(goblin.Health, Is.EqualTo(50));
                Assert.That(healButton.IsEnabled, Is.True, "Heal button should be enabled");
                Assert.That(fireballButton.IsEnabled, Is.True, "Fireball button should be enabled");
            });

            // Click heal button - should heal player
            Console.WriteLine("Action: Click Heal Button");
            healButton.Click();
            Console.WriteLine($"  {healthLabel.Text} (healed 30)");
            Console.WriteLine($"  {manaLabel.Text} (spent 20 mana)");
            Console.WriteLine($"  Heal Button Enabled: {healButton.IsEnabled} (on cooldown!)\n");

            Assert.Multiple(() =>
            {
                Assert.That(_playerModel.Health, Is.EqualTo(100), "Player should stay at max health (already at 100)");
                Assert.That(_playerModel.Mana, Is.EqualTo(80), "Mana should be deducted");
                Assert.That(healthLabel.Text, Is.EqualTo("Health: 100"), "Health label should update via binding");
                Assert.That(manaLabel.Text, Is.EqualTo("Mana: 80"), "Mana label should update via binding");
                Assert.That(healButton.IsEnabled, Is.False, "Button should be disabled during cooldown");
            });

            // Try to click heal button again - should be blocked by cooldown
            Console.WriteLine("Action: Try to click Heal Button again (should fail - cooldown)");
            var healthBefore = _playerModel.Health;
            var manaBefore = _playerModel.Mana;
            healButton.Click();
            Console.WriteLine($"  Health: {_playerModel.Health} (unchanged)");
            Console.WriteLine($"  Mana: {_playerModel.Mana} (unchanged)");
            Console.WriteLine($"  Heal Button Enabled: {healButton.IsEnabled}\n");

            Assert.Multiple(() =>
            {
                Assert.That(_playerModel.Health, Is.EqualTo(healthBefore), "Health should not change - cooldown active");
                Assert.That(_playerModel.Mana, Is.EqualTo(manaBefore), "Mana should not change - cooldown active");
                Assert.That(healButton.IsEnabled, Is.False, "Button should still be disabled");
            });

            // Click fireball button - should damage goblin
            Console.WriteLine("Action: Click Fireball Button (target: Goblin)");
            fireballButton.Click();
            Console.WriteLine($"  Goblin Health: {goblin.Health} (took 25 damage)");
            Console.WriteLine($"  {manaLabel.Text} (spent 15 mana)");
            Console.WriteLine($"  Fireball Button Enabled: {fireballButton.IsEnabled} (on cooldown!)\n");

            Assert.Multiple(() =>
            {
                Assert.That(goblin.Health, Is.EqualTo(25), "Goblin should take damage");
                Assert.That(_playerModel.Mana, Is.EqualTo(65), "Mana should be deducted");
                // Assert.That(goblinHealthLabel.Text, Is.EqualTo("Health: 25"), "Goblin health label should update");
                Assert.That(manaLabel.Text, Is.EqualTo("Mana: 65"), "Mana label should update");
                Assert.That(fireballButton.IsEnabled, Is.False, "Fireball button should be disabled during cooldown");
            });

            // Try clicking while on cooldown - should fail
            Console.WriteLine("Action: Spam Fireball Button (should fail - cooldown)");
            fireballButton.Click();
            fireballButton.Click();
            fireballButton.Click();
            Console.WriteLine($"  Goblin Health: {goblin.Health} (unchanged from spam clicks)");
            Console.WriteLine($"  Mana: {_playerModel.Mana} (unchanged)\n");

            Assert.That(goblin.Health, Is.EqualTo(25), "Spamming disabled button should not affect target");

            // Wait for cooldown and verify buttons re-enable
            Console.WriteLine("Action: Wait for cooldowns (simulating 2.5 seconds)...");
            System.Threading.Thread.Sleep(2500); // Wait for cooldowns
            healButton.UpdateEnabledState();
            fireballButton.UpdateEnabledState();
            Console.WriteLine($"  Heal Button Enabled: {healButton.IsEnabled} (cooldown expired!)");
            Console.WriteLine($"  Fireball Button Enabled: {fireballButton.IsEnabled} (cooldown expired!)\n");

            Assert.Multiple(() =>
            {
                Assert.That(healButton.IsEnabled, Is.True, "Heal button should re-enable after cooldown");
                Assert.That(fireballButton.IsEnabled, Is.True, "Fireball button should re-enable after cooldown");
            });

            // Final spell cast to verify it works again
            Console.WriteLine("Action: Click Fireball Button again (after cooldown)");
            fireballButton.Click();
            Console.WriteLine($"  Goblin Health: {goblin.Health} (took another 25 damage)");
            Console.WriteLine($"  Goblin is dead: {goblin.IsDead}\n");

            Assert.Multiple(() =>
            {
                Assert.That(goblin.Health, Is.EqualTo(0), "Goblin should be dead");
                Assert.That(goblin.IsDead, Is.True, "Goblin IsDead flag should be true");
                // Assert.That(goblinHealthLabel.Text, Is.EqualTo("Health: 0"));
            });

            Console.WriteLine("=== Demo Complete ===");
            Console.WriteLine("Two-way binding successfully demonstrated:");
            Console.WriteLine("  ✓ Button state syncs with command CanExecute");
            Console.WriteLine("  ✓ UI labels update automatically via bindings");
            Console.WriteLine("  ✓ Cooldowns prevent rapid casting");
            Console.WriteLine("  ✓ Mana is properly deducted");
            Console.WriteLine("  ✓ Damage/healing applied correctly");
        }
    }
}