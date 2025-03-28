using api.Data;
using api.Models.Entities;
using api.Services.Passwords;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        public UserRepository(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;

            // Auto migrate and add example users if they do not exist
            _context.Database.Migrate();

            var users = new[]
            {
            new { Email = "admin@gmail.com", Password = "123456", Role = "Admin" },
            new { Email = "admin2@gmail.com", Password = "123456", Role = "Admin" },
            new { Email = "admin3@gmail.com", Password = "123456", Role = "Admin" },
            new { Email = "ajay@gmail.com", Password = "123456", Role = "User" },
            new { Email = "user2@gmail.com", Password = "123456", Role = "User" },
            new { Email = "user3@gmail.com", Password = "123456", Role = "User" },
            new { Email = "super@gmail.com", Password = "123456", Role = "Super" }
            };

            foreach (var userInfo in users)
            {
            if (!_context.User.Any(u => u.Email == userInfo.Email))
            {
                var user = new User
                {
                Email = userInfo.Email,
                Password = _passwordHasher.HashPassword(userInfo.Password),
                Role = userInfo.Role
                };

                _context.Add(user);
            }
            }

            _context.SaveChanges();
        }
        public async Task<User> GetByEmailAsync(string email)
        {
            var user = await _context.User.FirstOrDefaultAsync(x => x.Email == email);

            return user;
        }
        public async Task<User> GetByRefreshTokenAsync(string refreshToken)
        {
            var user = await _context.User.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

            return user;

        }
        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
