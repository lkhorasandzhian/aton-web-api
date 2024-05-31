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
		/// <summary>
		/// Получить список всех пользователей (и активных, и неактивных).
		/// </summary>
		/// <remarks> Доступно только в режиме DEBUG. </remarks>
		/// <returns> Список всех пользователей. </returns>
		[HttpGet("debug/get-all-users")]
		public async Task<ActionResult<List<User>>> GetUsers()
		{
			return await _userService.GetUsersAsync();
		}
#endif

		/// <summary>
		/// Метод OPTIONS для предоставления информации о поддерживаемых методах и возможностях сервера.
		/// </summary>
		/// <returns> Список поддерживаемых методов и возможностей сервера. </returns>
		[HttpOptions("options")]
		[AllowAnonymous]
		public IActionResult GetAvailableMethods()
		{
			Response.Headers.Append("Allow", "GET, POST, PUT, DELETE, HEAD, OPTIONS");
			return Ok();
		}

		/// <summary>
		/// Метод HEAD для проверки доступности ресурса.
		/// </summary>
		/// <returns> Заголовки ответа без тела ответа. </returns>
		[HttpHead("head")]
		[AllowAnonymous]
		public IActionResult Head()
		{
			Response.Headers.Append("X-Resource-Type", "User");
			Response.Headers.Append("X-Resource-Author", "AtonWebAPI");
			return Ok();
		}

		/// <summary>
		/// 1) Создание пользователя по логину, паролю, имени, полу и дате рождения
		/// + указание будет ли пользователь админом (доступно админам).
		/// </summary>
		/// <param name="registration"> Введенные пользователем регистрационные данные из Swagger UI. </param>
		/// <returns> Зарегистрированный пользовательский профиль. </returns>
		[HttpPost("register")]
		public async Task<ActionResult<User>> Register([FromForm] Registration registration)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			string? creatorLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (registration.Admin && !User.IsInRole("Administrator"))
			{
				return StatusCode(StatusCodes.Status403Forbidden, "You don't have permission to assign Administrator status to a new user");
			}
			else if (await _userService.HasUserWithRequiredLoginAsync(registration.Login))
			{
				return Conflict("User with such login already exists");
			}

			var user = registration.CreateUserByGivenData(creatorLogin ?? string.Empty);

			await _userService.AddUserAsync(user);

			return CreatedAtAction(nameof(RequestByLogin), new { id = user.Guid }, user);
		}

		/// <summary>
		/// 2) Изменение имени, пола или даты рождения пользователя (может менять либо администратор,
		/// либо  лично пользователь, если он активен (отсутствует RevokedOn)).
		/// </summary>
		/// <param name="selectedUserLogin"> Логин пользователя, чей профиль требуется обновить. </param>
		/// <param name="name"> Обновлённое имя. </param>
		/// <param name="gender"> Обновлённый пол. </param>
		/// <param name="birthday"> Обновлённая дата рождения. </param>
		/// <returns> Статус операции. </returns>
		[Authorize(Roles = "User")]
		[HttpPut("change/profile-data/{selectedUserLogin}")]
		public async Task<ActionResult> ChangeProfileData(
			[FromRoute, Required]
			string selectedUserLogin,

			[FromQuery, RegularExpression("^[a-zA-Zа-яА-Я]*$", ErrorMessage = "Name must contain only Latin letters and Russian letters")]
			string? name,

			[FromQuery, Range(0, 2, ErrorMessage = "Gender value must be in range [0;2] (0 - female, 1 - male, 2 - undefined)")]
			int? gender,

			[FromQuery, DataType(DataType.Date, ErrorMessage = "Invalid date format"), DateNotInFuture]
			DateTime? birthday
			)
		{
			// Selected User - пользователь, которому меняют данные профиля.
			User? selectedUser = await _userService.GetUserByLoginAsync(selectedUserLogin);

			// Current User - текущий авторизованный пользователь, производящий изменения.
			string currentUserLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

			if (selectedUser == null)
			{
				return NotFound();
			}
			else if (!User.IsInRole("Administrator") &&
				(currentUserLogin != selectedUserLogin || selectedUser.RevokedOn.HasValue))
			{
				return StatusCode(StatusCodes.Status403Forbidden, "You do not have permission to change someone else's data");
			}

			_userService.UpdateUserData(selectedUser, name, gender, birthday);
			
			_userService.MarkUserAsModified(selectedUser, currentUserLogin);
			return Ok();
		}

		/// <summary>
		/// 3) Изменение пароля (пароль может менять либо администратор,
		/// либо лично пользователь, если он активен (отсутствует RevokedOn)). 
		/// </summary>
		/// <param name="selectedUserLogin"> Логин пользователя, чей пароль требуется обновить. </param>
		/// <param name="password"> Обновлённый пароль. </param>
		/// <returns> Статус операции. </returns>
		[Authorize(Roles = "User")]
		[HttpPut("change/password/{selectedUserLogin}")]
		public async Task<ActionResult> ChangeProfileData(
			[FromRoute, Required]
			string selectedUserLogin,

			[FromQuery, Required, DataType(DataType.Password), RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Password must contain only Latin letters and numbers")]
			string password
			)
		{
			// Selected User - пользователь, которому меняют пароль.
			User? selectedUser = await _userService.GetUserByLoginAsync(selectedUserLogin);

			// Current User - текущий авторизованный пользователь, производящий изменения.
			string currentUserLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

			if (selectedUser == null)
			{
				return NotFound();
			}
			else if (!User.IsInRole("Administrator") &&
				(currentUserLogin != selectedUserLogin || selectedUser.RevokedOn.HasValue))
			{
				return StatusCode(StatusCodes.Status403Forbidden, "You do not have permission to change someone else's password");
			}

			_userService.UpdateUserPassword(selectedUser, password);

			_userService.MarkUserAsModified(selectedUser, currentUserLogin);
			return Ok();
		}

		/// <summary>
		/// 4) Изменение логина (логин может менять либо администратор,
		/// либо лично пользователь, если  он активен (отсутствует RevokedOn),
		/// логин должен оставаться уникальным).
		/// </summary>
		/// <param name="selectedUserLogin"> Логин пользователя, чей логин требуется обновить. </param>
		/// <param name="newUserLogin"> Обновлённый логин. </param>
		/// <returns> Статус операции. </returns>
		[Authorize(Roles = "User")]
		[HttpPut("change/login/{selectedUserLogin}")]
		public async Task<ActionResult> ChangeLogin(
			[FromRoute, Required]
			string selectedUserLogin,

			[FromQuery, Required, RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Login must contain only Latin letters and numbers")]
			string newUserLogin
			)
		{
			// Selected User - пользователь, которому меняют логин.
			User? selectedUser = await _userService.GetUserByLoginAsync(selectedUserLogin);

			// Current User - текущий авторизованный пользователь, производящий изменения.
			string currentUserLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

			if (selectedUser == null)
			{
				return NotFound();
			}
			else if (!User.IsInRole("Administrator") &&
				(currentUserLogin != selectedUserLogin || selectedUser.RevokedOn.HasValue))
			{
				return StatusCode(StatusCodes.Status403Forbidden, "You do not have permission to change someone else's password");
			}
			else if (await _userService.HasUserWithRequiredLoginAsync(newUserLogin))
			{
				return Conflict("This login is already taken.");
			}

			_userService.UpdateUserLogin(selectedUser, newUserLogin);

			_userService.MarkUserAsModified(selectedUser, currentUserLogin);
			return Ok();
		}

		/// <summary>
		/// 5) Запрос списка всех активных (отсутствует RevokedOn) пользователей.
		/// Список отсортирован по CreatedOn (доступно админам).
		/// </summary>
		/// <returns> Отсортированный список активных пользователей. </returns>
		[Authorize(Roles = "Administrator")]
		[HttpGet("request/all-active-users")]
		public async Task<ActionResult<List<User>>> RequestAllActiveUsers() =>
			await _userService.GetActiveUsersAsync();

		/// <summary>
		/// 6) Запрос пользователя по логину. В списке должны быть имя, пол,
		/// дата рождения и статус активный или нет (доступно админам).
		/// </summary>
		/// <param name="login"> Запрошенный логин. </param>
		/// <returns> Имя, пол, дата рождения, статус активности.  </returns>
		[Authorize(Roles = "Administrator")]
		[HttpGet("request/by-login")]
		public async Task<ActionResult<User>> RequestByLogin([FromQuery, Required] string login)
		{
			var user = await _userService.GetUserByLoginAsync(login);
			return user != null ?
				Ok(new List<object?>
				{
					user.Name,
					user.Gender,
					user.Birthday,
					user.RevokedBy == null
				}) :
				NotFound();
		}

		/// <summary>
		/// 7) Запрос пользователя по логину и паролю (доступно только самому пользователю,
		/// если он активен (отсутствует RevokedOn)).
		/// </summary>
		/// <param name="login"> Логин запрошенного пользователя. </param>
		/// <param name="password"> Пароль запрошенного пользователя. </param>
		/// <returns> Личный профиль пользователя. </returns>
		[Authorize(Roles = "User")]
		[HttpGet("request/personal-profile")]
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

		/// <summary>
		/// 8) Запрос всех пользователей старше определённого возраста (доступно Админам).
		/// </summary>
		/// <param name="age"> Запрошенный возраст. </param>
		/// <returns> Пользователи старше указанного возраста. </returns>
		[Authorize(Roles = "Administrator")]
		[HttpGet("request/users-over-specified-age")]
		public async Task<ActionResult<List<User>?>> RequestUsersOverSpecifiedAge([FromQuery, Required] int age)
		{
			if (age <= 0)
			{
				return BadRequest("Age must be greater than zero");
			}

			return await _userService.GetUsersOverSpecifiedAgeAsync(age);
		}

		/// <summary>
		/// 9) Удаление пользователя по логину полное или мягкое (при мягком удалении
		/// должна происходить простановка RevokedOn и RevokedBy). (доступно админам) 
		/// </summary>
		/// <param name="login"></param>
		/// <param name="isHardDelete"> true - полное удаление из БД,
		/// false - мягкое удаление через маркировку Revoked. </param>
		/// <returns> Статус операции. </returns>
		[Authorize(Roles = "Administrator")]
		[HttpDelete("delete")]
		public async Task<ActionResult> DeleteUser([FromQuery, Required] string login, [FromQuery, Required] bool isHardDelete)
		{
			// Current User - текущий авторизованный администратор, производящий удаление.
			string currentAdminLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

			if (!await _userService.HasUserWithRequiredLoginAsync(login))
			{
				return NotFound();
			}

			await _userService.DeleteUserAsync(login, isHardDelete, currentAdminLogin);

			return Ok();
		}

		/// <summary>
		/// 10) Восстановление пользователя - очистка 
		/// полей (RevokedOn, RevokedBy) (доступно админам).
		/// </summary>
		/// <param name="login"> Логин пользователя, которого требуется восстановить. </param>
		/// <returns> Статус операции. </returns>
		[Authorize(Roles = "Administrator")]
		[HttpPut("restore/user/{login}")]
		public async Task<ActionResult> RestoreUser([FromRoute, Required] string login)
		{
			var user = await _userService.GetUserByLoginAsync(login);

			if (user == null)
			{
				return NotFound();
			}

			if (user.RevokedOn == null && user.RevokedBy == null)
			{
				return BadRequest("User is already active");
			}

			user.RevokedOn = null;
			user.RevokedBy = null;

			return Ok("User has been restored successfully");
		}
	}
}
