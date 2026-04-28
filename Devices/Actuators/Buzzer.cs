using StadiumSystem.Audio;
using StadiumSystem.Events;

namespace StadiumSystem.Devices.Actuators;

public class Buzzer : IActuator, IAudioDevice
{
    public bool IsOn { get; private set; }

    public void On() { }
    public void Off() { }
    public void Tone(int hz) { }
    public void NoTone() { }


    public void Play(Enums.SoundTracks track) { }
    public void Stop(Enums.SoundTracks track) { }
    public void SetVolume(double volume) { }
}
