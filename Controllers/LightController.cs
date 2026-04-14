using StadiumSystem.Domain;
using StadiumSystem.Events;

namespace StadiumSystem.Controllers;

/// <summary>
/// GRASP - Controller: coordina el comportamiento de las luces del estadio
/// ante distintos modos de operación. Alta cohesión: solo maneja luces.
/// </summary>
public class LightController : IEventHandler
{
    private List<Light> _lights;

    public LightController()
    {
        _lights = new List<Light>();
    }

    /// <summary>Enciende todas las luces.</summary>
    public void TurnLightsOn() { }

    /// <summary>Apaga todas las luces.</summary>
    public void TurnLightsOff() { }

    /// <summary>Hace parpadear todas las luces (p.ej. celebración de gol).</summary>
    public void BlinkLights() { }

    /// <summary>Activa el patrón de luces de emergencia.</summary>
    public void EmergencyLights() { }

    /// <inheritdoc/>
    public void Handle(IEvent @event) { }
}
