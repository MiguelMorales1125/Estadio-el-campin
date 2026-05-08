using Microsoft.Extensions.DependencyInjection;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.Infrastructure.Security;
using StadiumSystem.Services;

var services = new ServiceCollection();
services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
services.AddScoped<AppDbContext>();
services.AddScoped<AuthService>();

using var provider = services.BuildServiceProvider();
using (var scope = provider.CreateScope())
{
	var auth = scope.ServiceProvider.GetRequiredService<AuthService>();
	auth.SeedAdminIfNotExists();
}

System.Console.WriteLine("Servicios registrados y admin creado si faltaba.");
