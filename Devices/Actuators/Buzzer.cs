using StadiumSystem.Audio;
using StadiumSystem.Domain.Events;
using StadiumSystem.Domain.Enums;

namespace StadiumSystem.Devices.Actuators;

public class Buzzer : IActuator, IAudioDevice
{
    public int Pin { get; set; }
    public bool IsOn { get; private set; }

    public void On()
    {
        IsOn = true;
    }

    public void Off()
    {
        IsOn = false;
    }

    public void Tone(int hz)
    {
        On();
    }

    public void NoTone()
    {
        Off();
    }

    public void Play(SoundTracks track)
    {
        On();
    }

    public void Stop(SoundTracks track)
    {
        Off();
    }

    public void SetVolume(double volume) { }
}
