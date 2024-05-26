using Microsoft.AspNetCore.Mvc;
using AtonWebAPI.Models;
using Microsoft.EntityFrameworkCore;

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
	}
}
