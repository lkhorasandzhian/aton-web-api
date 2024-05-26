﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using AtonWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AtonWebAPI
{
	public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		private readonly StorageContext _context;

		[Obsolete(message: "ISystemClock type is obsolete. Should be changed to TimeProvider")]
		public BasicAuthenticationHandler(
			IOptionsMonitor<AuthenticationSchemeOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock,
			StorageContext context)
			: base(options, logger, encoder, clock)
		{
			_context = context;
		}

		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			if (!Request.Headers.ContainsKey("Authorization"))
			{
				return AuthenticateResult.Fail("Missing Authorization Header");
			}

			try
			{
				var authHeader = AuthenticationHeaderValue.Parse(Request.Headers.Authorization!);
				var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
				var credentials = Encoding.UTF8.GetString(credentialBytes).Split([':'], 2);
				var username = credentials[0];
				var password = credentials[1];

				var user = await _context.Users.SingleOrDefaultAsync(u => u.Login == username && u.Password == password);

				if (user == null)
				{
					return AuthenticateResult.Fail("Invalid Username or Password");
				}

				var claims = new[]
				{
				new Claim(ClaimTypes.NameIdentifier, user.Login),
				new Claim(ClaimTypes.Name, user.Name),
			};
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