using StadiumSystem.Domain;
using StadiumSystem.Enums;
using StadiumSystem.Events;
using StadiumSystem.Infrastructure;

namespace StadiumSystem.Controllers;


public class StadiumController : IEventHandler
{
    private Stadium _stadium;
    private SoundController _soundController;
    private ScoreController _scoreController;
    private LightController _lightController;
    private EventBus _eventBus;

    public StadiumController()
    {
        _stadium        = new Stadium();
        _soundController = new SoundController();
        _scoreController = new ScoreController();
        _lightController = new LightController();
        _eventBus = EventBus.GetInstance();
    }


    public void SetMode(StadiumStates state) { }

    public void ActivateMatchMode() { }

    public void ActivateMaintenanceMode() { }

    public void ActivateEmergencyMode() { }

    public void Handle(IEvent @event) { }
}
