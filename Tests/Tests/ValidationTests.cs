using Xunit;

namespace Tests
{
    public class ValidationTests
    {
        [Fact]
        public void EmptyUsername_Invalid()
        {
            string username = "";
            bool isValid = !string.IsNullOrWhiteSpace(username);
            Assert.False(isValid);
        }

        [Fact]
        public void ValidUsername_Valid()
        {
            string username = "admin";
            bool isValid = !string.IsNullOrWhiteSpace(username);
            Assert.True(isValid);
        }

        [Fact]
        public void ShortPassword_Invalid()
        {
            string password = "123";
            bool isValid = password.Length >= 6;
            Assert.False(isValid);
        }

        [Fact]
        public void LongPassword_Valid()
        {
            string password = "secure123";
            bool isValid = password.Length >= 6;
            Assert.True(isValid);
        }

        [Fact]
        public void ValidEmail_Valid()
        {
            string email = "user@test.com";
            bool isValid = email.Contains("@") && email.Contains(".");
            Assert.True(isValid);
        }

        [Fact]
        public void InvalidEmail_Invalid()
        {
            string email = "invalid-email";
            bool isValid = email.Contains("@") && email.Contains(".");
            Assert.False(isValid);
        }
    }
}