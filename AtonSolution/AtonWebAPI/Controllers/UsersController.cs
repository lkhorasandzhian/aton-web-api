using Microsoft.AspNetCore.Mvc;
using AtonWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AtonWebAPI.Models.Validations;
using System.Security.Claims;

namespace AtonWebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly StorageContext _context;

		public UsersController(StorageContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<User>>> GetUsers()
		{
			return await _context.Users.ToListAsync();
		}

		[HttpPost("Register")]
		[Authorize(Roles = "Administrator")]
		public async Task<ActionResult<User>> RegisterUser([FromForm] Registration registration)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// Получаем логин текущего авторизованного пользователя
			string? creatorLogin = User.FindFirst(ClaimTypes.Name)?.Value;

			if (string.IsNullOrEmpty(creatorLogin))
			{
				return Unauthorized("Unable to identify the creator of the user");
			}

			var user = registration.CreateUserByGivenData(creatorLogin);

			_context.Users.Add(user);
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetUsers), new { id = user.Guid }, user);
		}
	}
}
