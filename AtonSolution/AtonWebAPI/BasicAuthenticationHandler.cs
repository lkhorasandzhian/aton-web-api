using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using AtonWebAPI.Services;

namespace AtonWebAPI
{
	public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		private readonly IUserService _service;

		[Obsolete(message: "ISystemClock type is obsolete. Should be changed to TimeProvider")]
		public BasicAuthenticationHandler(
			IOptionsMonitor<AuthenticationSchemeOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock,
			IUserService service)
			: base(options, logger, encoder, clock)
		{
			_service = service;
		}

		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			if (!Request.Headers.ContainsKey("Authorization"))
			{
				return AuthenticateResult.NoResult();
			}

			try
			{
				var authHeader = AuthenticationHeaderValue.Parse(Request.Headers.Authorization!);
				var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
				var credentials = Encoding.UTF8.GetString(credentialBytes).Split([':'], 2);
				var login = credentials.FirstOrDefault();
				var password = credentials.LastOrDefault();

				var user = await _service.AuthenticateAsync(login, password);

				if (user == null)
				{
					return AuthenticateResult.Fail("Invalid Username or Password");
				}
				else if (user.RevokedOn != null)
				{
					return AuthenticateResult.Fail($"User was revoked on {user.RevokedOn}");
				}

				var claims = new List<Claim>()
				{
					new(ClaimTypes.NameIdentifier, user.Login),
					new(ClaimTypes.Name, user.Name),
					new(ClaimTypes.Role, "User")
				};

				if (user.Admin)
				{
					claims.Add(new(ClaimTypes.Role, "Administrator"));
				}

				var identity = new ClaimsIdentity(claims, Scheme.Name);
				var principal = new ClaimsPrincipal(identity);
				var ticket = new AuthenticationTicket(principal, Scheme.Name);

				return AuthenticateResult.Success(ticket);
			}
			catch
			{
				return AuthenticateResult.Fail("Invalid Authorization Header");
			}
		}
	}
}