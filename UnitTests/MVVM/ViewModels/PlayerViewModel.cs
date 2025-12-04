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
        [Inject] private readonly PlayerModel? _model;
        public Bindable<int> Health => _model!.Health;
        public Bindable<int> Mana => _model!.Mana;

        public PlayerViewModel()
        {
            DI.InjectDependencies(this);
        }
    }
}
