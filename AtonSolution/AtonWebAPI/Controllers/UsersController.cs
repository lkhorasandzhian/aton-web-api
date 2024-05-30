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

		/// <summary>
		/// 1) Создание пользователя по логину, паролю, имени, полу и дате рождения
		/// + указание будет ли пользователь админом (доступно админам).
		/// </summary>
		/// <param name="registration"> Введенные пользователем регистрационные данные из Swagger UI. </param>
		/// <returns> Зарегистрированный пользовательский профиль. </returns>
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

		/// <summary>
		/// 5) Запрос списка всех активных (отсутствует RevokedOn) пользователей.
		/// Список отсортирован по CreatedOn (доступно админам).
		/// </summary>
		/// <returns> Отсортированный список активных пользователей. </returns>
		[Authorize(Roles = "Administrator")]
		[HttpGet("Request_all_active_users")]
		public async Task<ActionResult<List<User>>> RequestAllActiveUsers() =>
			await _userService.GetActiveUsersAsync();

		/// <summary>
		/// 6) Запрос пользователя по логину. В списке должны быть имя, пол,
		/// дата рождения и статус активный или нет (доступно админам).
		/// </summary>
		/// <param name="login"> Запрошенный логин. </param>
		/// <returns> Имя, пол, дата рождения, статус активности.  </returns>
		[Authorize(Roles = "Administrator")]
		[HttpGet("Request_by_login")]
		public async Task<ActionResult<User>> RequestByLogin([FromQuery, Required] string login)
		{
			var user = await _userService.GetUserByLoginAsync(login);
			return user != null ?
				Ok(new List<object>
				{
					user.Name,
					user.Gender,
					user.Birthday,
					user.RevokedBy == null
				}) :
				NotFound();
		}

		[Authorize(Roles = "User")]
		[HttpGet("Request_personal_profile")]
		public async Task<ActionResult<User>> RequestPersonalProfile(
			[FromQuery, Required] string login,
			[FromQuery, Required, DataType(DataType.Password)] string password
			)
		{
			string currentUserLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
			User currentUser = (await _userService.GetUserByLoginAsync(currentUserLogin))!;

			User? requestedUser = await _userService.GetUserByLoginAndPasswordAsync(login, password);

			if (requestedUser == null || currentUser.Login != login || currentUser.Password != password)
			{
				return NotFound();
			}

			return Ok(requestedUser);
		}
	}
}
