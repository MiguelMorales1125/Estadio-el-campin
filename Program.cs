using Microsoft.Extensions.DependencyInjection;
using StadiumSystem.Controllers;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.Infrastructure.Security;
using StadiumSystem.Services;
using StadiumSystem.UI;
using StadiumSystem.UI.Theming;

DotNetEnv.Env.Load();
ThemeManager.ConfigureFromEnvironment();

var services = new ServiceCollection();
services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
services.AddScoped<AppDbContext>();
services.AddScoped<AuthService>();
services.AddScoped<UserService>();
services.AddScoped<AuthController>();
services.AddScoped<UserController>();
services.AddScoped<ConsoleApp>();

using var provider = services.BuildServiceProvider();
using (var scope = provider.CreateScope())
{
	var app = scope.ServiceProvider.GetRequiredService<ConsoleApp>();
	app.Run();
}
