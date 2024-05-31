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

		public async Task<bool> HasUserWithRequiredLoginAsync(string login) =>
			await _context.Users.AnyAsync(x => x.Login == login);

		public void MarkUserAsModified(User user, string editorLogin)
		{
			user.ModifiedOn = DateTime.Now;
			user.ModifiedBy = editorLogin;
		}

		public void UpdateUserData(User user, string? name, int? gender, DateTime? birthday)
		{
			if (name != null)
			{
				user.Name = name;
			}

			if (gender != null)
			{
				user.Gender = gender.Value;
			}

			user.Birthday = birthday;
		}

		public void UpdateUserPassword(User user, string password) =>
			user.Password = password;

		public void UpdateUserLogin(User user, string login) =>
			user.Login = login;

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

		public async Task<List<User>?> GetUsersOverSpecifiedAgeAsync(int age) =>
			await _context.Users.Where(u => u.Birthday.HasValue && DateTime.Compare(u.Birthday.Value, DateTime.Now.AddYears(-(age + 1))) < 0).ToListAsync();

		public async Task<int> DeleteUserAsync(string login, bool isHardDelete, string adminLogin)
		{
			var user = (await _context.Users.SingleOrDefaultAsync(u => u.Login == login))!;

            if (isHardDelete)
            {
                _context.Users.Remove(user);
            }
			else
			{
				user.RevokedOn = DateTime.Now;
				user.RevokedBy = adminLogin;
			}

			return await _context.SaveChangesAsync();
		}
	}
}
