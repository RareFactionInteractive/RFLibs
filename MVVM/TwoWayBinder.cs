namespace RFLibs.MVVM
{   
    public abstract class TwoWayBinder<T> : Binder<T>
    {
        protected Bindable<T> Source;
        private bool _isUpdatingUI;
        private bool _isUpdatingModel;

        protected void BindTwoWay(Bindable<T> source)
        {
            Source = source;

            // Model → UI
            Source.OnValueChanged += value =>
            {
                if (_isUpdatingModel) return;

                _isUpdatingUI = true;
                UpdateUI(value);
                _isUpdatingUI = false;
            };

            // Initialize UI
            UpdateUI(Source.Value);
        }

        protected void UpdateModel(T value)
        {
            if (_isUpdatingUI) return;

            _isUpdatingModel = true;
            Source.Value = value;
            _isUpdatingModel = false;
        }

        protected abstract void UpdateUI(T value);
    }
}
