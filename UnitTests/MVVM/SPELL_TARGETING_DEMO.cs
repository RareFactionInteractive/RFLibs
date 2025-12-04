using System;
using RFLibs.DependencyInjection;
using UnitTests.MVVM;
using UnitTests.MVVM.Models;
using UnitTests.MVVM.Spells;
using UnitTests.MVVM.ViewModels;

namespace SpellTargetingExample
{
    /// <summary>
    /// Example demonstrating all three spell targeting methods
    /// </summary>
    public class SpellTargetingDemo
    {
        public void RunDemo()
        {
            // Setup
            DI.Bind<UnitTests.MVVM.TimeProvider>(() => DateTime.Now.Ticks);
            var player = new PlayerModel(health: 80, mana: 100);
            DI.Bind(player);

            var goblin = new Goblin();  // 50 HP
            var orc = new Orc();        // 100 HP

            // Create spells
            var fireball = new SpellViewModel(
                new DamageSpell("Fireball", manaCost: 20, cooldownSeconds: 1.0f, damage: 25)
            );

            var heal = new SpellViewModel(
                new HealSpell("Heal", manaCost: 15, cooldownSeconds: 0.5f, healAmount: 30)
            );

            Console.WriteLine("=== Spell Targeting Demo ===\n");

            // METHOD 1: Explicit Targeting - Direct control
            Console.WriteLine("1. EXPLICIT TARGETING");
            Console.WriteLine($"   Goblin HP: {goblin.Health.Value}");
            Console.WriteLine("   Casting Fireball on Goblin...");
            fireball.CastOnTargetCommand.Execute(goblin);
            Console.WriteLine($"   Goblin HP: {goblin.Health.Value} (took 25 damage)");
            Console.WriteLine($"   Player Mana: {player.Mana.Value}\n");

            // METHOD 2: Default Target - Tab-targeting style
            Console.WriteLine("2. DEFAULT TARGET (Tab-Targeting)");
            Console.WriteLine($"   Orc HP: {orc.Health.Value}");
            Console.WriteLine("   Setting Orc as default target...");
            fireball.SetTarget(orc);
            Console.WriteLine("   Casting Fireball (uses default target)...");
            fireball.CastCommand.Execute();
            Console.WriteLine($"   Orc HP: {orc.Health.Value} (took 25 damage)");
            Console.WriteLine($"   Player Mana: {player.Mana.Value}\n");

            // METHOD 3: Self-Cast - No target defaults to player
            Console.WriteLine("3. SELF-CAST (Player Healing)");
            Console.WriteLine($"   Player HP: {player.Health.Value}");
            Console.WriteLine("   Casting Heal (no target = self-cast)...");
            heal.CastCommand.Execute();
            Console.WriteLine($"   Player HP: {player.Health.Value} (healed 30)");
            Console.WriteLine($"   Player Mana: {player.Mana.Value}\n");

            // BONUS: Can also explicitly self-target
            Console.WriteLine("BONUS: Explicit Self-Targeting");
            Console.WriteLine($"   Player HP: {player.Health.Value}");
            Console.WriteLine("   Explicitly targeting player with Heal...");
            heal.CastOnTargetCommand.Execute(player);
            Console.WriteLine($"   Player HP: {player.Health.Value} (capped at 100)");
            Console.WriteLine($"   Player Mana: {player.Mana.Value}\n");

            // Show final state
            Console.WriteLine("=== Final State ===");
            Console.WriteLine($"Player - HP: {player.Health.Value}/100, Mana: {player.Mana.Value}/100");
            Console.WriteLine($"Goblin - HP: {goblin.Health.Value}/50 {(goblin.IsDead ? "[DEAD]" : "")}");
            Console.WriteLine($"Orc    - HP: {orc.Health.Value}/100");
        }
    }

    /* Expected Output:
     * 
     * === Spell Targeting Demo ===
     * 
     * 1. EXPLICIT TARGETING
     *    Goblin HP: 50
     *    Casting Fireball on Goblin...
     *    Goblin HP: 25 (took 25 damage)
     *    Player Mana: 80
     * 
     * 2. DEFAULT TARGET (Tab-Targeting)
     *    Orc HP: 100
     *    Setting Orc as default target...
     *    Casting Fireball (uses default target)...
     *    Orc HP: 75 (took 25 damage)
     *    Player Mana: 60
     * 
     * 3. SELF-CAST (Player Healing)
     *    Player HP: 80
     *    Casting Heal (no target = self-cast)...
     *    Player HP: 100 (healed 30, capped at max)
     *    Player Mana: 45
     * 
     * BONUS: Explicit Self-Targeting
     *    Player HP: 100
     *    Explicitly targeting player with Heal...
     *    Player HP: 100 (already at max)
     *    Player Mana: 30
     * 
     * === Final State ===
     * Player - HP: 100/100, Mana: 30/100
     * Goblin - HP: 25/50
     * Orc    - HP: 75/100
     */
}
