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
            const int initialVal = 0;
            const int testVal = 42;
            var callbackOldVal = -1;
            var callbackNewVal = -1;
            var didEventFire = false;

            Bindable<int> bindableInt = new(initialVal);
            bindableInt.OnValueChanged += (oldVal, newVal) =>
            {
                didEventFire = true;
                callbackOldVal = oldVal;
                callbackNewVal = newVal;
            };

            bindableInt.Value = testVal;

            Assert.Multiple(() =>
            {
                Assert.That(didEventFire, Is.True);
                Assert.That(bindableInt.Value, Is.EqualTo(testVal));
                Assert.That(callbackOldVal, Is.EqualTo(initialVal), "Old value should match initial value");
                Assert.That(callbackNewVal, Is.EqualTo(testVal), "New value should match test value");
            });
        }
    }
}