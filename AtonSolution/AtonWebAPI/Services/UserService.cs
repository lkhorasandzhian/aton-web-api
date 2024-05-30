using AtonWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AtonWebAPI.Services
{
	public class UserService : IUserService
	{
		private readonly StorageContext _context;

		public UserService(StorageContext context)
		{
			_context = context;
		}

		public async Task<List<User>> GetUsersAsync()
		{
			return await _context.Users.ToListAsync();
		}

		public async Task<List<User>> GetActiveUsersAsync() =>
			await _context.Users
						  .Where(u => u.RevokedOn == null)
						  .OrderBy(u => u.CreatedOn)
						  .ToListAsync();

		public async Task<User?> AuthenticateAsync(string? login, string? password) =>
			await _context.Users.SingleOrDefaultAsync(u => u.Login == login && u.Password == password);

		public async Task<int> AddUserAsync(User user)
		{
			_context.Users.Add(user);
			return await _context.SaveChangesAsync();
		}

		public async Task<User?> GetUserByLoginAsync(string? login) =>
			await _context.Users.SingleOrDefaultAsync(u => u.Login == login);

		public async Task<User?> GetUserByLoginAndPasswordAsync(string? login, string? password) =>
			await _context.Users.SingleOrDefaultAsync(u => u.Login == login && u.Password == password);
	}
}
