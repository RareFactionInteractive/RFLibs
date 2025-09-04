using RFLibs.Bindable;

namespace UnitTests;

public class BindableTests
{
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