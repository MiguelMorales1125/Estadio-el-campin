using StadiumSystem.Enums;

namespace StadiumSystem.Audio;

/// <summary>
/// GRASP - Protected Variations: desacopla SoundController del dispositivo
/// de audio concreto (p.ej. Buzzer, speaker externo, etc.).
/// </summary>
public interface IAudioDevice
{
    void Play(SoundTracks track);
    void Stop(SoundTracks track);
    void SetVolume(double volume);
}
