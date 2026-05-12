using Microsoft.Extensions.DependencyInjection;
using StadiumSystem.Controllers;
using StadiumSystem.Infrastructure;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.Infrastructure.Security;
using StadiumSystem.Services;
using StadiumSystem.UI;
using StadiumSystem.UI.Theming;
using StadiumSystem.Devices;

DotNetEnv.Env.Load();
ThemeManager.ConfigureFromEnvironment();

var services = new ServiceCollection();
services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
services.AddScoped<AppDbContext>();
services.AddScoped<AuditLogService>();
services.AddScoped<AuthService>();
services.AddScoped<UserService>();
services.AddScoped<AuthController>();
services.AddScoped<UserController>();
services.AddSingleton<ArduinoConnection>();
services.AddSingleton<DeviceFactory>();
services.AddSingleton<IDeviceRegistry, DeviceRegistry>();
services.AddSingleton<IDeviceDiscoveryService, DeviceDiscoveryService>();
services.AddScoped<LightController>();
services.AddSingleton<ArduinoRuntimeProcessor>();
services.AddScoped<StadiumController>();
services.AddScoped<ConsoleApp>();
services.AddScoped<ScoreController>(sp => new ScoreController(
    sp.GetRequiredService<IDeviceRegistry>(),
    sp.GetRequiredService<ArduinoConnection>(),
    sp.GetRequiredService<ITerminalLogService>()));
services.AddScoped<SoundController>();
services.AddSingleton<ITerminalLogService>(new TerminalLogService(LogLevel.Info));

using var provider = services.BuildServiceProvider();
using (var scope = provider.CreateScope())
{
	var runtimeProcessor = provider.GetRequiredService<ArduinoRuntimeProcessor>();
	var stadiumController = scope.ServiceProvider.GetRequiredService<StadiumController>();
	var arduinoConnection = provider.GetRequiredService<ArduinoConnection>();
	var discoveryService = provider.GetRequiredService<IDeviceDiscoveryService>();

	arduinoConnection.StartListening();

    
	try
	{
		var discoveryCompleted = await discoveryService.DiscoverDevicesAsync();
		if (discoveryCompleted)
		{
			Console.WriteLine("[Startup] Dispositivos descubiertos exitosamente");
		}
		else
		{
			Console.WriteLine("[Startup] Advertencia: No se pudieron descubrir dispositivos");
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine($"[Startup] Error en discovery: {ex.Message}");
	}

	var app = scope.ServiceProvider.GetRequiredService<ConsoleApp>();
	app.Run();
}
