using AtonWebAPI.Models;

namespace AtonWebAPI.Services
{
	public interface IUserService
	{
		Task<bool> HasUserWithRequiredLoginAsync(string login);
		void MarkUserAsModified(User user, string editorLogin);
		void UpdateUserData(User user, string? name, int? gender, DateTime? birthday);
		void UpdateUserPassword(User user, string password);
		void UpdateUserLogin(User user, string login);
		Task<List<User>> GetUsersAsync();
		Task<List<User>> GetActiveUsersAsync();
		Task<User?> AuthenticateAsync(string? login, string? password);
		Task<int> AddUserAsync(User user);
		Task<User?> GetUserByLoginAsync(string? login);
		Task<User?> GetUserByLoginAndPasswordAsync(string? login, string? password);
		Task<List<User>?> GetUsersOverSpecifiedAgeAsync(int age);
		Task<int> DeleteUserAsync(string login, bool isHardDelete, string adminLogin);
	}
}
