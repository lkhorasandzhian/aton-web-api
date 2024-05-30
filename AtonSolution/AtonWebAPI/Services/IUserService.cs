using AtonWebAPI.Models;

namespace AtonWebAPI.Services
{
	public interface IUserService
	{
		Task<List<User>> GetUsersAsync();
		Task<List<User>> GetActiveUsersAsync();
		Task<User?> AuthenticateAsync(string? login, string? password);
		Task<int> AddUserAsync(User user);
		Task<User?> GetUserByLoginAsync(string? login);
		Task<User?> GetUserByLoginAndPasswordAsync(string? login, string? password);
		Task<List<User>?> GetUsersOverSpecifiedAgeAsync(int age);
	}
}
