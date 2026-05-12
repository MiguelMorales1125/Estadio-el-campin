# AGENTS.md — StadiumSystem

## Project

C# .NET 10 console app (`StadiumSystem.csproj`) — football stadium control system with Arduino integration. Bidirectional serial communication: sensors (PIR, LDR) and actuators (LED, Buzzer, LCD) connected to Arduino, coordinated from C#. No tests, no CI, no linter/formatter configured.

**Design principles**: Follow SOLID, GRASP, and design patterns. Prioritize clean architecture and best practices in all changes.

## Run

- **Quick start**: `run.bat` (Windows) or `run.sh` (Linux) — starts Docker, applies migrations, builds, runs.
- **Manual**:
  1. `docker-compose up -d` (PostgreSQL 16 on port 5432, container `estadio_db`)
  2. `dotnet ef database update` (apply EF Core migrations)
  3. `dotnet run`

## Env

- `.env` file loaded by `DotNetEnv` at runtime from `AppContext.BaseDirectory`.
- **DB**: `DB_HOST`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`.
- **Arduino**: `COM_PORT` (default `COM3`), `COM_BAUD` (default `9600`).
- **Auth**: `ASPNETCORE_ENVIRONMENT` or `DOTNET_ENVIRONMENT`, `ADMIN_PASSWORD`.
- **UI**: `UI_THEME` (optional).
- `.env` is gitignored. Copy `.env.example` as starting point.
- `SERIAL_PORT`, `ARDUINO_GOAL_TEAM`, `ARDUINO_GOAL_COOLDOWN_MS` exist in `.env` but are **not read** by the codebase.

## DB

- PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`.
- DbContext: `Infrastructure/Data/AppDbContext.cs` — reads `.env` from `AppContext.BaseDirectory` (not project root).
- Entities: `User`, `CommandAuditLog`, `StadiumState`, `MatchSession`.
- Migrations live in `Migrations/`. Create new: `dotnet ef migrations add <Name>`.

## Architecture

```
Program.cs              — entry point, DI setup, starts Arduino + discovery, launches ConsoleApp
Domain/                 — entities (User, CommandAuditLog, StadiumState, MatchSession), enums, events. Pure domain logic, no infrastructure dependencies
Infrastructure/         — EF Core DbContext, Arduino serial comms (ArduinoConnection, RealSerialPortAdapter), device discovery, security (BcryptPasswordHasher), EventBus
Services/               — business logic layer: AuthService (login, admin seed), UserService, AuditLogService, TerminalLogService. Depends on Domain + Infrastructure
Controllers/            — orchestration layer: coordinates Services + Devices for specific domains (Auth, User, Stadium, Light, Score, Sound). Subscribes to domain events
Devices/                — hardware abstraction: DeviceFactory, Sensors (PIR, LDR), Actuators (LED, Buzzer, LCD). Sends typed Commands to Arduino via ArduinoConnection (no raw string formatting)
UI/                     — Spectre.Console TUI: ConsoleApp (main loop), Menus (MainMenu, SessionMenu), Screens (Login, StadiumControl, MatchManagement, UserManagement, TerminalLogs), Theming
Commands/               — Command pattern: ICommand with Serialize(). Concrete commands (LedOnCommand, LedOffCommand, RequestInventoryCommand) encapsulate the serial protocol. Devices create commands; ArduinoConnection (invoker) serializes and sends them
Tools/                  — Arduino serial simulator (Python), virtual serial pair scripts (Linux: socat, Windows: com0com + PowerShell)
```

**Flow**: `Program.cs` → DI setup → `ArduinoConnection.StartListening()` → `DeviceDiscoveryService` → `ConsoleApp.Run()` → Login → SessionMenu → Screens → Controllers → Services/Devices → Arduino

## Arduino / Serial

- `ArduinoConnection` opens serial port from `COM_PORT` env var. Singleton pattern.
- If no physical Arduino: run `Tools/serial_arduino_sim.py` (requires `pyserial`). Create virtual serial pair with `Tools/virtual_serial_pair.sh` (Linux, socat) or `Tools/virtual_serial_pair.ps1` (Windows, com0com, auto-elevates to admin).
- Code always attempts real serial connection — no mock logic is implemented.
- Device discovery runs at startup via `DeviceDiscoveryService` — queries Arduino inventory via `REQUEST_INVENTORY` command.

## Key quirks

- **No test project** — verify by running the app manually.
- **EF Core 10** — use `dotnet ef` CLI tools (package reference already included).
- **Spectre.Console** for TUI — theme configurable via env (see `UI/Theming/ThemeManager.cs`).
- **Audio** directory exists but only has `IAudioDevice.cs` interface — not wired into DI.
- DI registration is manual in `Program.cs` — add new services there.
- `ArduinoConnection` has a static `GetInstance()` singleton alongside DI registration — be careful not to create duplicate instances.
