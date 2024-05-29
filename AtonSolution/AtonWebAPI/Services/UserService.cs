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

		public async Task<User?> AuthenticateAsync(string? login, string? password) =>
			await _context.Users.SingleOrDefaultAsync(u => u.Login == login && u.Password == password);
	}
}
