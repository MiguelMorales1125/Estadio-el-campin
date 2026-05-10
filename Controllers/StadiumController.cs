using StadiumSystem.Domain;
using StadiumSystem.Domain.Enums;
using StadiumSystem.Domain.Events;
using StadiumSystem.Infrastructure.Events;
using StadiumSystem.Infrastructure;

namespace StadiumSystem.Controllers;


public class StadiumController : IEventHandler
{
    private Stadium _stadium;
    private SoundController _soundController;
    private ScoreController _scoreController;
    private LightController _lightController;
    private EventBus _eventBus;

    public StadiumController(IDeviceRegistry deviceRegistry, ScoreController scoreController, SoundController soundController, LightController lightController)
    {
        _stadium = new Stadium();
        _soundController = soundController;
        _scoreController = scoreController;
        _lightController = lightController;
        _eventBus = EventBus.GetInstance();
        _eventBus.Subscribe("MovementDetected", e => Handle(e));
    }


    public void SetMode(StadiumStates state) { }

    public void ActivateMatchMode() { }

    public void ActivateMaintenanceMode() { }

    public void ActivateEmergencyMode() { }

    public void Handle(IEvent @event)
    {
        if (@event == null) return;
        _scoreController.Handle(@event);
        _soundController.Handle(@event);
        _lightController.Handle(@event);
    }
}
