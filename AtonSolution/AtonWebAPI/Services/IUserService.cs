using AtonWebAPI.Models;

namespace AtonWebAPI.Services
{
	public interface IUserService
	{
		Task<User?> AuthenticateAsync(string? login, string? password);
	}
}
