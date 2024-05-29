using AtonWebAPI.Models;

namespace AtonWebAPI.Services
{
	public interface IUserService
	{
		Task<List<User>> GetUsersAsync();
		Task<User?> AuthenticateAsync(string? login, string? password);
		Task<int> AddUserAsync(User user);
		Task<User?> GetUserByLoginAsync(string? login);
	}
}
