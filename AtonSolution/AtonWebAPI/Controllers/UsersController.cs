using Microsoft.AspNetCore.Mvc;
using AtonWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using AtonWebAPI.Models.Validations;
using System.Security.Claims;
using AtonWebAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace AtonWebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly IUserService _userService;

		public UsersController(IUserService userService)
		{
			_userService = userService;
		}

#if DEBUG
		[HttpGet]
		public async Task<ActionResult<List<User>>> GetUsers()
		{
			return await _userService.GetUsersAsync();
		}
#endif

		[HttpPost("Register")]
		public async Task<ActionResult<User>> Register([FromForm] Registration registration)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			string? creatorLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(creatorLogin))
			{
				return Unauthorized("Unable to identify the creator of the user");
			}
			else if (registration.Admin && !User.IsInRole("Administrator"))
			{
				return StatusCode(StatusCodes.Status403Forbidden, "You don't have permission to assign Administrator status to a new user");
			}

			var user = registration.CreateUserByGivenData(creatorLogin);

			await _userService.AddUserAsync(user);

			return CreatedAtAction(nameof(RequestByLogin), new { id = user.Guid }, user);
		}

		[Authorize(Roles = "Administrator")]
		[HttpGet("Request_by_login")]
		public async Task<ActionResult<User>> RequestByLogin([FromQuery, Required] string login)
		{
			var user = await _userService.GetUserByLoginAsync(login);
			return user != null ? Ok(user) : NotFound();
		}
	}
}
