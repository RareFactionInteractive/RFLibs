using NUnit.Framework;
using RFLibs.Bindable;

namespace UnitTests
{
    public class BindableTests
    {
        internal class DummyPersonModel
        {
            public string Name = "Adam";
            public int Age = 42;

            public DummyPersonModel()
            {
            }

            public DummyPersonModel(DummyPersonModel other)
            {
                Name = other.Name;
                Age = other.Age;
            }
        }

        internal class DummyPersonView
        {
            public void OnNameChanged(string newName)
            {
            }

            public void OnAgeChanged(int newAge)
            {
            }
        }

        internal class DummyPersonViewModel
        {
            private readonly DummyPersonModel _model;

            public DummyPersonViewModel(DummyPersonModel model)
            {
                _model = model;
            }
        }

        [Test, Order(0)]
        public void BindableNotifiesWhenValueChanges()
        {
            const int testVal = 42;
            var callbackVal = 0;
            var didEventFire = false;

            Bindable<int> bindableInt = new(0);
            bindableInt.OnValueChanged += i =>
            {
                didEventFire = true;
                callbackVal = i;
            };

            bindableInt.Value = testVal;

            Assert.Multiple(() =>
            {
                Assert.That(didEventFire, Is.True);
                Assert.That(bindableInt.Value, Is.EqualTo(testVal));
                Assert.That(callbackVal, Is.EqualTo(bindableInt.Value));
            });
        }
    }
}