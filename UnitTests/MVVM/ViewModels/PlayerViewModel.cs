using System.ComponentModel;
using RFLibs.MVVM;
using RFLibs.DependencyInjection;
using RFLibs.DependencyInjection.Attributes;
using UnitTests.MVVM.Models;

namespace UnitTests.MVVM.ViewModels
{
    [Scope(Scope.Global)]
    [Lifetime(Lifetime.Singleton)]
    public class PlayerViewModel
    {
        [Inject] private readonly PlayerModel _model;
        public Bindable<int> Health;
        public Bindable<int> Mana;
        
        public PlayerViewModel()
        {
            DI.InjectDependencies(this);

            Health = new Bindable<int>(_model.Health, newHealth => _model.Health = newHealth);
            Mana = new Bindable<int>(_model.Mana, newMana => _model.Mana = newMana);

            // Subscribe to model changes to keep bindables in sync
            _model.PropertyChanged += OnModelPropertyChanged;
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PlayerModel.Health):
                    Health.Value = _model.Health;
                    break;
                case nameof(PlayerModel.Mana):
                    Mana.Value = _model.Mana;
                    break;
            }
        }
    }
}
