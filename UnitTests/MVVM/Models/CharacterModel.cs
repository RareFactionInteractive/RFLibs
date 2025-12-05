using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UnitTests.MVVM.Models
{
    /// <summary>
    /// Base class for all character models (player and enemies).
    /// Provides common character properties and implements INotifyPropertyChanged
    /// to keep ViewModels in sync when model state changes.
    /// </summary>
    public abstract class CharacterModel : ITargetable, INotifyPropertyChanged
    {
        private int _health;
        private int _mana;
        private int _maxHealth;
        private int _maxMana;
        private int _attackPower;

        public string Name { get; protected set; }
        public int Damage { get; protected set; }

        public int Health
        {
            get => _health;
            set => SetProperty(ref _health, value);
        }

        public int MaxHealth
        {
            get => _maxHealth;
            set => SetProperty(ref _maxHealth, value);
        }

        public int Mana
        {
            get => _mana;
            set => SetProperty(ref _mana, value);
        }

        public int MaxMana
        {
            get => _maxMana;
            set => SetProperty(ref _maxMana, value);
        }

        public int AttackPower
        {
            get => _attackPower;
            set => SetProperty(ref _attackPower, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected CharacterModel(string name, int maxHealth, int attackPower, int maxMana = 0)
        {
            Name = name;
            _health = maxHealth;
            _maxHealth = maxHealth;
            _attackPower = attackPower;
            _mana = _maxMana;
            _maxMana = maxMana;
        }

        public void TakeDamage(int amount)
        {
            Health = Math.Max(0, Health - amount);
        }

        public void Heal(int amount)
        {
            Health = Math.Min(MaxHealth, Health + amount);
        }

        public bool SpendMana(int amount)
        {
            if (Mana < amount) return false;
            Mana -= amount;
            return true;
        }

        public void AddMana(int amount)
        {
            Mana = Math.Min(MaxMana, Mana + amount);
        }

        public void Attack(ITargetable target)
        {
            target.TakeDamage(AttackPower);
        }

        public bool IsDead => Health <= 0;

        /// <summary>
        /// Sets a property and raises PropertyChanged if the value actually changed.
        /// </summary>
        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
