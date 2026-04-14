using StadiumSystem.Domain;
using StadiumSystem.Enums;
using StadiumSystem.Events;
using StadiumSystem.Infrastructure;

namespace StadiumSystem.Controllers;

/// <summary>
/// GRASP - Controller (Facade Controller): punto de entrada principal para
/// los casos de uso del estadio. Delega responsabilidades específicas a
/// sub-controladores (High Cohesion + Low Coupling).
/// </summary>
public class StadiumController : IEventHandler
{
    private Stadium _stadium;
    private SoundController _soundController;
    private ScoreController _scoreController;
    private LightController _lightController;
    private AdminSession _adminSession;
    private EventBus _eventBus;

    public StadiumController()
    {
        _stadium        = new Stadium();
        _soundController = new SoundController();
        _scoreController = new ScoreController();
        _lightController = new LightController();
        _adminSession = new AdminSession();
        _eventBus = new EventBus();
    }

    /// <summary>
    /// Cambia el modo del estadio y delega al método de activación
    /// correspondiente según el estado recibido.
    /// </summary>
    public void SetMode(StadiumStates state) { }

    /// <summary>Activa el modo partido: luces, música y marcador listos.</summary>
    public void ActivateMatchMode() { }

    /// <summary>Activa el modo mantenimiento: luces reducidas, sin audio.</summary>
    public void ActivateMaintenanceMode() { }

    /// <summary>Activa el modo emergencia: sirenas, luces de emergencia.</summary>
    public void ActivateEmergencyMode() { }

    /// <inheritdoc/>
    public void Handle(IEvent @event) { }
}
