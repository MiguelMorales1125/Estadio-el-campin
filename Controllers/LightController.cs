using StadiumSystem.Domain;
using StadiumSystem.Events;

namespace StadiumSystem.Controllers;


public class LightController : IEventHandler
{
    private List<Light> _lights;

    public LightController()
    {
        _lights = new List<Light>();
    }

    public void TurnLightsOn() { }

    public void TurnLightsOff() { }

    public void BlinkLights() { }

    public void EmergencyLights() { }

    public void Handle(IEvent @event) { }
}
