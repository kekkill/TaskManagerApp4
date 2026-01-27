using Xunit;
using System.Security.Cryptography;
using System.Text;

namespace Tests
{
    public class PasswordHashTests
    {
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        [Fact]
        public void SamePassword_SameHash()
        {
            var hash1 = HashPassword("test123");
            var hash2 = HashPassword("test123");
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void DifferentPasswords_DifferentHashes()
        {
            var hash1 = HashPassword("password1");
            var hash2 = HashPassword("password2");
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void EmptyPassword_HashesCorrectly()
        {
            var hash = HashPassword("");
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }
    }
}