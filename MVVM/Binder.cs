using RFLibs.DependencyInjection;
using RFLibs.DependencyInjection.Attributes;

namespace RFLibs.MVVM
{
    public abstract class Binder<T>
    {
        [Inject] protected T ViewModel { get; set; }

        protected virtual void OnEnable()
        {
            DI.InjectDependencies(this);
            OnBind();
        }

        protected abstract void OnBind();
    }
}