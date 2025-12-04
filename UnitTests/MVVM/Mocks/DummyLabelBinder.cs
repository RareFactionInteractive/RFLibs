using RFLibs.MVVM;
using UnitTests.MVVM.ViewModels;

namespace UnitTests.MVVM.Mocks
{
    /// <summary>
    /// Mock binder for testing UI label binding
    /// Demonstrates automatic UI updates via two-way binding
    /// </summary>
    public class DummyLabelBinder : Binder<PlayerViewModel>
    {
        public string Text { get; private set; } = "";

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
}
