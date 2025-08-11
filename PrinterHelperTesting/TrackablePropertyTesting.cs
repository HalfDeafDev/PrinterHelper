using PrinterHelper.Models;
using PrinterHelper.Helpers;

namespace PrinterHelperTesting
{
    public class TrackablePropertyTesting
    {
        [Fact]
        public void AlwaysTrue()
        {
            Assert.True(true);
        }

        [Fact]
        public void TrackableProperty_HasChanged_IsFalse_OnInitialization ()
        {
            TrackableProperty<int> trackable = new(10);
            Assert.False(trackable.HasChanged());
        }

        [Fact]
        public void TrackableProperty_HasChanged_IsTrue_AfterChange()
        {
            TrackableProperty<int> trackable = new(10);
            trackable.Value = 20;
            Assert.True(trackable.HasChanged());
        }

        [Fact]
        public void TrackableProperty_HasChanged_IsFalse_AfterChangeSolidified()
        {
            TrackableProperty<int> trackable = new(10);
            trackable.Value = 20;
            trackable.Solidify();
            Assert.False(trackable.HasChanged());
        }

        [Fact]
        public void TrackableProperty_HasChanged_IsFalse_AfterSolidifyWithValue()
        {
            TrackableProperty<int> trackable = new(10);
            trackable.Solidify(20);
            Assert.False(trackable.HasChanged());
        }

        [Theory]
        [InlineData(6)]
        [InlineData(8)]
        [InlineData(-3)]
        [InlineData(-9)]
        public void TrackableProperty_Value_IsCorrect_AfterChange(int value)
        {
            TrackableProperty<int> trackable = new(10);
            trackable.Value = value;
            Assert.True(trackable.Value == value);
        }

        [Theory]
        [InlineData(6)]
        [InlineData(8)]
        [InlineData(-3)]
        [InlineData(-9)]
        public void TrackableProperty_Value_IsCorrect_AfterSolidifedChange(int value)
        {
            TrackableProperty<int> trackable = new(10);
            trackable.Solidify(value);
            Assert.True(trackable.Value == value);
        }
    }
}