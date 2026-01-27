using System.Security.Cryptography;
using System.Text;
using TaskManagerApp.Data;
using TaskManagerApp.Models;

namespace TaskManagerApp.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public bool RegisterUser(string username, string password, string email)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
                return false;

            if (_context.Users.Any(u => u.Username == username || u.Email == email))
                return false;

            var passwordHash = HashPassword(password);

            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                Email = email
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return true;
        }

        public User? Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return null;

            var inputHash = HashPassword(password);
            return inputHash == user.PasswordHash ? user : null;
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return string.Empty;

            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
        public bool IsAdmin(User user)
        {
            return user.Role == "Admin";
        }
    }
}