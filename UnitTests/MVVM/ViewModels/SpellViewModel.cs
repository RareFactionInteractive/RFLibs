using RFLibs.MVVM;
using RFLibs.DependencyInjection;
using RFLibs.DependencyInjection.Attributes;
using RFLibs.MVVM.Interfaces;
using UnitTests.MVVM;
using UnitTests.MVVM.Models;
using UnitTests.MVVM.Spells;

namespace UnitTests.MVVM.ViewModels
{
    public class SpellViewModel
    {
        [Inject] private readonly PlayerModel? _player;
        [Inject] private readonly TimeProvider? _timeProvider;

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
            return _player!.Mana >= _model.ManaCost &&
                   _model.IsOffCooldown(_timeProvider!());
        }

        private bool CanCastOnTarget(ITargetable target)
        {
            return target != null &&
                   _player!.Mana >= _model.ManaCost &&
                   _model.IsOffCooldown(_timeProvider!());
        }

        private void Cast()
        {
            if (!CanCast()) return;

            _player!.Mana -= _model.ManaCost;
            _model.MarkCast(_timeProvider!());

            // Apply effect to current target if set, otherwise to player (self-cast)
            ApplyEffect(_currentTarget ?? _player!);
        }

        private void CastOnTarget(ITargetable target)
        {
            if (!CanCastOnTarget(target)) return;

            _player!.Mana -= _model.ManaCost;
            _model.MarkCast(_timeProvider!());

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
}
