using System;
using RFLibs.MVVM;
using RFLibs.DependencyInjection.Attributes;
using RFLibs.DependencyInjection;
using RFLibs.MVVM.Interfaces;

namespace UnitTests.MVVM
{
    public delegate long TimeProvider();

    public interface IHealable { void Heal(int amount); }
    public interface IDamageable { void TakeDamage(int amount); }
    public interface ITargetable : IHealable, IDamageable { }

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

    [Scope(Scope.Global)]
    [Lifetime(Lifetime.Singleton)]
    public class PlayerViewModel
    {
        [Inject] private readonly PlayerModel _model;
        public Bindable<int> Health => _model.Health;
        public Bindable<int> Mana => _model.Mana;

        public PlayerViewModel()
        {
            DI.InjectDependencies(this);
        }
    }

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

    public class SpellViewModel
    {
        [Inject] private readonly PlayerModel _player;
        [Inject] private readonly TimeProvider _timeProvider;

        private SpellModel _model;
        private ITargetable? _currentTarget;

        public string Name => _model.Name;
        public ICommand CastCommand { get; }
        public ICommand<ITargetable> CastOnTargetCommand { get; }

        public SpellViewModel(SpellModel model)
        {
            _model = model;

            CastCommand = new Command(
                execute: Cast,
                canExecute: CanCast);

            CastOnTargetCommand = new Command<ITargetable>(
                execute: CastOnTarget,
                canExecute: CanCastOnTarget);

            DI.InjectDependencies(this);
        }

        public void SetTarget(ITargetable target)
        {
            _currentTarget = target;
        }

        private bool CanCast()
        {
            return _player.Mana.Value >= _model.ManaCost &&
                   _model.IsOffCooldown(_timeProvider());
        }

        private bool CanCastOnTarget(ITargetable target)
        {
            return target != null &&
                   _player.Mana.Value >= _model.ManaCost &&
                   _model.IsOffCooldown(_timeProvider());
        }

        private void Cast()
        {
            if (!CanCast()) return;

            _player.Mana.Value -= _model.ManaCost;
            _model.MarkCast(_timeProvider());

            // Apply effect to current target if set, otherwise to player (self-cast)
            ApplyEffect(_currentTarget ?? _player);
        }

        private void CastOnTarget(ITargetable target)
        {
            if (!CanCastOnTarget(target)) return;

            _player.Mana.Value -= _model.ManaCost;
            _model.MarkCast(_timeProvider());

            ApplyEffect(target);
        }

        private void ApplyEffect(ITargetable target)
        {
            switch (_model.EffectType)
            {
                case SpellModel.SpellEffectType.Heal:
                    target.Heal(_model.EffectMagnitude);
                    break;
                case SpellModel.SpellEffectType.Damage:
                    target.TakeDamage(_model.EffectMagnitude);
                    break;
                    // Add other effect types as needed
            }
        }
    }

    public abstract class EnemyModel : ITargetable
    {
        public string Name { get; protected set; }
        public int Damage { get; protected set; }
        public Bindable<int> Health { get; protected set; }
        public int MaxHealth { get; protected set; }

        protected EnemyModel(string name, int damage, int maxHealth)
        {
            Name = name;
            Damage = damage;
            MaxHealth = maxHealth;
            Health = new Bindable<int>(maxHealth);
        }

        public void TakeDamage(int amount)
        {
            Health.Value = Math.Max(0, Health.Value - amount);
        }

        public void Heal(int amount)
        {
            Health.Value = Math.Min(MaxHealth, Health.Value + amount);
        }

        public bool IsDead => Health.Value <= 0;
    }

    public class Goblin : EnemyModel
    {
        public Goblin() : base("Goblin", 10, 50) { }
    }

    public class Orc : EnemyModel
    {
        public Orc() : base("Orc", 20, 100) { }
    }

    public class EnemyController
    {
        private readonly EnemyModel _model;

        public EnemyController(EnemyModel model)
        {
            _model = model;
        }

        public void Attack(IDamageable target)
        {
            target.TakeDamage(_model.Damage);
        }
    }

    public class DummyLabelBinder : Binder<PlayerViewModel>
    {
        public string Text { get; private set; }

        public DummyLabelBinder()
        {
            //We are outside of Unity, so OnEnable is never called.
            base.OnEnable();
            UpdateHealth(ViewModel.Health.Value);
        }

        protected override void OnBind()
        {
            ViewModel.Health.OnValueChanged += UpdateHealth;
        }

        private void UpdateHealth(int health)
        {
            Text = $"Health: {health}";
        }

        ~DummyLabelBinder()
        {
            ViewModel.Health.OnValueChanged -= UpdateHealth;
        }
    }

    /// <summary>
    /// Mock button that binds to a spell command
    /// Demonstrates command binding with CanExecute
    /// </summary>
    public class MockSpellButton
    {
        private readonly ICommand? _command;
        private readonly ICommand<ITargetable>? _targetedCommand;
        private readonly ITargetable? _target;

        public bool IsEnabled { get; private set; }

        // Constructor for non-targeted commands
        public MockSpellButton(ICommand command)
        {
            _command = command;
            _command.CanExecuteChanged += UpdateEnabledState;
            UpdateEnabledState();
        }

        // Constructor for targeted commands
        public MockSpellButton(ICommand<ITargetable> targetedCommand, ITargetable target)
        {
            _targetedCommand = targetedCommand;
            _target = target;
            _targetedCommand.CanExecuteChanged += UpdateEnabledState;
            UpdateEnabledState();
        }

        public void Click()
        {
            if (_command != null && _command.CanExecute)
            {
                _command.Execute();
                UpdateEnabledState();
            }
            else if (_targetedCommand != null && _targetedCommand.CanExecute(_target))
            {
                _targetedCommand.Execute(_target);
                UpdateEnabledState();
            }
        }

        public void UpdateEnabledState()
        {
            if (_command != null)
            {
                IsEnabled = _command.CanExecute;
            }
            else if (_targetedCommand != null)
            {
                IsEnabled = _targetedCommand.CanExecute(_target);
            }
        }

        ~MockSpellButton()
        {
            if (_command != null)
                _command.CanExecuteChanged -= UpdateEnabledState;
            if (_targetedCommand != null)
                _targetedCommand.CanExecuteChanged -= UpdateEnabledState;
        }
    }

    /// <summary>
    /// Mock label that binds to a Bindable value
    /// Demonstrates automatic UI updates via two-way binding
    /// </summary>
    public class MockLabel
    {
        public string Text { get; private set; } = "";
        private string _labelType = "";

        public void BindToValue(Bindable<int> bindable, string label)
        {
            _labelType = label;

            // Subscribe to value changes
            bindable.OnValueChanged += UpdateText;

            // Initialize with current value
            UpdateText(bindable.Value);
        }

        private void UpdateText(int value)
        {
            Text = $"{_labelType}: {value}";
        }
    }
}