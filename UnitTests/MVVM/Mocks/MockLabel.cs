using RFLibs.MVVM;

namespace UnitTests.MVVM.Mocks
{
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
