using StadiumSystem.Audio;
using StadiumSystem.Events;

namespace StadiumSystem.Devices.Actuators;

/// <summary>
/// GRASP - Information Expert: conoce cómo emitir tonos.
/// Implementa IActuator e IAudioDevice para ser usado por SoundController.
/// </summary>
public class Buzzer : IActuator, IAudioDevice
{
    public bool IsOn { get; private set; }

    public void On() { }
    public void Off() { }

    /// <summary>Emite un tono a la frecuencia especificada en Hz.</summary>
    public void Tone(int hz) { }

    /// <summary>Detiene el tono activo.</summary>
    public void NoTone() { }

    // IAudioDevice
    public void Play(Enums.SoundTracks track) { }
    public void Stop(Enums.SoundTracks track) { }
    public void SetVolume(double volume) { }
}
