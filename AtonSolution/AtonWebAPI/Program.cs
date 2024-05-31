using AtonWebAPI;
using AtonWebAPI.Models;
using AtonWebAPI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddDbContext<StorageContext>(opt => opt.UseInMemoryDatabase("UserList"));
builder.Services.AddAuthentication("BasicAuthentication")
	.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

builder.Services.AddAuthorizationBuilder()
	.AddPolicy("Administrator", policy => policy.RequireRole("Administrator"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "basic",
		In = ParameterLocation.Header,
		Description = "Basic Authorization header using the Bearer scheme."
	});

	options.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "basic"
				}
			},
			Array.Empty<string>()
		}
	});

	var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database initialization.
using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<StorageContext>();

	if (!context.Users.Any())
	{
		var admin = new User
		{
			Login = "admin",
			Password = "admin123",
			Name = "Администратор",
			Gender = 2,
			Birthday = DateTime.Now.AddYears(-20),
			Admin = true,
			CreatedOn = DateTime.Now,
			CreatedBy = string.Empty
		};

		context.Users.Add(admin);
		context.SaveChanges();
	}
}

app.Run();
