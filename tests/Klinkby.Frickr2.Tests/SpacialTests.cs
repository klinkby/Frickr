namespace Klinkby.Frickr2.Tests
{
    public class SpacialTests
    {
        [Theory]
        [InlineData(0f, 0f, 0f, 0)]
        [InlineData(0f, 0f, 3.6f, 0.001f)]
        [InlineData(38f, 53f, 22.927f, 38.8897f)]
        public void Convert_to_expected_value(float degrees, float minutes, float seconds, float dd)
        {
            Assert.Equal((degrees, minutes, seconds), Spacial.DecimalDegreesToDms(dd));
        }
    }
}