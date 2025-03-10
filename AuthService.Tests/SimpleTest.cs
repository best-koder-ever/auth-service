using Xunit;

namespace UserService.Tests
{
    public class SimpleTests
    {
        [Fact]
        public void TestTrueIsTrue()
        {
            // Arrange
            bool condition = true;

            // Act & Assert
            Assert.True(condition);
        }
    }
}