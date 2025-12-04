using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RFLibs.DependencyInjection;
using UnitTests.MVVM.Models;
using UnitTests.MVVM.ViewModels;
using UnitTests.MVVM.Spells;
using UnitTests.MVVM.Mocks;

namespace UnitTests.MVVM
{
    public class MvvmTests
    {
        private readonly EnemyController _goblinController = new(new Goblin());
        private readonly EnemyController _orcController = new(new Orc());
        private DummyLabelBinder _dummyLabelBinder;

        [OneTimeSetUp]
        public void Setup()
        {
            DI.Bind<TimeProvider>(() => DateTime.Now.Ticks);
            DI.Bind(new PlayerModel(100, 100));  // Bind with 100 mana for testing
            DI.Bind(new PlayerViewModel());
            _dummyLabelBinder = new();
        }

        [SetUp]
        public void ResetPlayerForEachTest()
        {
            // Reset player stats before each test
            var player = DI.Resolve<PlayerModel>().Ok;
            if (player != null)
            {
                player.Health.Value = 100;
                player.Mana.Value = 100;
            }
        }

        [Test, Order(0)]
        public void TestPlayerModel()
        {
            Assert.Multiple(() =>
            {
                Assert.That(DI.Resolve<PlayerViewModel>().Ok.Health.Value, Is.EqualTo(100));
                Assert.That(_dummyLabelBinder.Text, Is.EqualTo("Health: 100"));
            });
        }

        [Test, Order(1)]
        public void ModelTakesDamageWhenControllerAttacks()
        {
            _goblinController.Attack(DI.Resolve<PlayerModel>().Ok);
            Assert.Multiple(() =>
            {
                Assert.That(DI.Resolve<PlayerViewModel>().Ok.Health.Value, Is.EqualTo(90));
                Assert.That(_dummyLabelBinder.Text, Is.EqualTo("Health: 90"));
            });

            _orcController.Attack(DI.Resolve<PlayerModel>().Ok);
            Assert.Multiple(() =>
            {
                Assert.That(DI.Resolve<PlayerViewModel>().Ok.Health.Value, Is.EqualTo(70));
                Assert.That(_dummyLabelBinder.Text, Is.EqualTo("Health: 70"));
            });
        }

        [Test, Order(2)]
        public void PlayerCanTargetEnemyWithSpell()
        {
            // Create enemy - reuse the player from OneTimeSetUp (reset by SetUp method)
            var goblin = new Goblin();
            var player = DI.Resolve<PlayerModel>().Ok;

            // Create a damage spell
            var fireballModel = new DamageSpell("Fireball", manaCost: 20, cooldownSeconds: 1.0f, damage: 15);
            var fireballViewModel = new SpellViewModel(fireballModel);

            // Player casts fireball on goblin
            Assert.That(goblin.Health.Value, Is.EqualTo(50), "Goblin should start with 50 HP");
            fireballViewModel.CastOnTargetCommand.Execute(goblin);

            Assert.Multiple(() =>
            {
                Assert.That(goblin.Health.Value, Is.EqualTo(35), "Goblin should have 35 HP after taking 15 damage");
                Assert.That(player.Mana.Value, Is.EqualTo(80), "Player should have spent 20 mana");
            });

            // Create a heal spell and test healing the enemy
            var healModel = new HealSpell("Minor Heal", manaCost: 10, cooldownSeconds: 0.5f, healAmount: 10);
            var healViewModel = new SpellViewModel(healModel);

            healViewModel.CastOnTargetCommand.Execute(goblin);
            Assert.Multiple(() =>
            {
                Assert.That(goblin.Health.Value, Is.EqualTo(45), "Goblin should have 45 HP after healing");
                Assert.That(player.Mana.Value, Is.EqualTo(70), "Player should have spent another 10 mana");
            });
        }

        [Test, Order(3)]
        public async Task PlayerCanTargetThemselfWithSpell()
        {
            // Reuse the player from OneTimeSetUp (reset by SetUp method)
            var player = DI.Resolve<PlayerModel>().Ok;

            // Create a heal spell
            var healModel = new HealSpell("Minor Heal", manaCost: 15, cooldownSeconds: 0.5f, healAmount: 20);
            var healViewModel = new SpellViewModel(healModel);

            // Method 1: Explicitly target the player
            healViewModel.CastOnTargetCommand.Execute(player);

            Assert.Multiple(() =>
            {
                Assert.That(player.Health.Value, Is.EqualTo(100), "Player should stay at 100 HP (already at max after healing 20)");
                Assert.That(player.Mana.Value, Is.EqualTo(85), "Player should have spent 15 mana");
            });

            await Task.Delay((int)(healModel.CooldownSeconds * 1000));

            // Method 2: Self-cast (no target set defaults to player)
            healViewModel.CastCommand.Execute();

            Assert.Multiple(() =>
            {
                Assert.That(player.Health.Value, Is.EqualTo(100), "Player should stay at 100 HP (already at max)");
                Assert.That(player.Mana.Value, Is.EqualTo(70), "Player should have spent another 15 mana");
            });

            await Task.Delay((int)(healModel.CooldownSeconds * 1000));

            // Verify healing doesn't exceed max health
            healViewModel.CastCommand.Execute();
            Assert.That(player.Health.Value, Is.EqualTo(100), "Player health should cap at 100");
        }

        [Test, Order(4)]
        public void MockButtonDemonstratesTwoWayBindingWithSpellCasting()
        {
            // Setup with reused player from OneTimeSetUp (reset by SetUp method)
            var player = DI.Resolve<PlayerModel>().Ok;
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
            healthLabel.BindToValue(player.Health, "Health");
            var manaLabel = new MockLabel();
            manaLabel.BindToValue(player.Mana, "Mana");

            // Create UI element that binds to goblin health
            var goblinHealthLabel = new MockLabel();
            goblinHealthLabel.BindToValue(goblin.Health, "Health");

            Console.WriteLine("=== Mock Button Two-Way Binding Demo ===\n");

            // Initial state
            Console.WriteLine("Initial State:");
            Console.WriteLine($"  {healthLabel.Text}");
            Console.WriteLine($"  {manaLabel.Text}");
            Console.WriteLine($"  {goblinHealthLabel.Text}");
            Console.WriteLine($"  Heal Button Enabled: {healButton.IsEnabled}");
            Console.WriteLine($"  Fireball Button Enabled: {fireballButton.IsEnabled}\n");

            Assert.Multiple(() =>
            {
                Assert.That(healthLabel.Text, Is.EqualTo("Health: 100"));
                Assert.That(manaLabel.Text, Is.EqualTo("Mana: 100"));
                Assert.That(goblinHealthLabel.Text, Is.EqualTo("Health: 50"));
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
                Assert.That(player.Health.Value, Is.EqualTo(100), "Player should stay at max health (already at 100)");
                Assert.That(player.Mana.Value, Is.EqualTo(80), "Mana should be deducted");
                Assert.That(healthLabel.Text, Is.EqualTo("Health: 100"), "Health label should update via binding");
                Assert.That(manaLabel.Text, Is.EqualTo("Mana: 80"), "Mana label should update via binding");
                Assert.That(healButton.IsEnabled, Is.False, "Button should be disabled during cooldown");
            });

            // Try to click heal button again - should be blocked by cooldown
            Console.WriteLine("Action: Try to click Heal Button again (should fail - cooldown)");
            var healthBefore = player.Health.Value;
            var manaBefore = player.Mana.Value;
            healButton.Click();
            Console.WriteLine($"  Health: {player.Health.Value} (unchanged)");
            Console.WriteLine($"  Mana: {player.Mana.Value} (unchanged)");
            Console.WriteLine($"  Heal Button Enabled: {healButton.IsEnabled}\n");

            Assert.Multiple(() =>
            {
                Assert.That(player.Health.Value, Is.EqualTo(healthBefore), "Health should not change - cooldown active");
                Assert.That(player.Mana.Value, Is.EqualTo(manaBefore), "Mana should not change - cooldown active");
                Assert.That(healButton.IsEnabled, Is.False, "Button should still be disabled");
            });

            // Click fireball button - should damage goblin
            Console.WriteLine("Action: Click Fireball Button (target: Goblin)");
            fireballButton.Click();
            Console.WriteLine($"  {goblinHealthLabel.Text} (took 25 damage)");
            Console.WriteLine($"  {manaLabel.Text} (spent 15 mana)");
            Console.WriteLine($"  Fireball Button Enabled: {fireballButton.IsEnabled} (on cooldown!)\n");

            Assert.Multiple(() =>
            {
                Assert.That(goblin.Health.Value, Is.EqualTo(25), "Goblin should take damage");
                Assert.That(player.Mana.Value, Is.EqualTo(65), "Mana should be deducted");
                Assert.That(goblinHealthLabel.Text, Is.EqualTo("Health: 25"), "Goblin health label should update");
                Assert.That(manaLabel.Text, Is.EqualTo("Mana: 65"), "Mana label should update");
                Assert.That(fireballButton.IsEnabled, Is.False, "Fireball button should be disabled during cooldown");
            });

            // Try clicking while on cooldown - should fail
            Console.WriteLine("Action: Spam Fireball Button (should fail - cooldown)");
            fireballButton.Click();
            fireballButton.Click();
            fireballButton.Click();
            Console.WriteLine($"  Goblin Health: {goblin.Health.Value} (unchanged from spam clicks)");
            Console.WriteLine($"  Mana: {player.Mana.Value} (unchanged)\n");

            Assert.That(goblin.Health.Value, Is.EqualTo(25), "Spamming disabled button should not affect target");

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
            Console.WriteLine($"  {goblinHealthLabel.Text} (took another 25 damage)");
            Console.WriteLine($"  Goblin is dead: {goblin.IsDead}\n");

            Assert.Multiple(() =>
            {
                Assert.That(goblin.Health.Value, Is.EqualTo(0), "Goblin should be dead");
                Assert.That(goblin.IsDead, Is.True, "Goblin IsDead flag should be true");
                Assert.That(goblinHealthLabel.Text, Is.EqualTo("Health: 0"));
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