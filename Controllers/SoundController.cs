using StadiumSystem.Audio;
using StadiumSystem.Enums;
using StadiumSystem.Events;

namespace StadiumSystem.Controllers;

/// <summary>
/// GRASP - Controller: coordina la reproducción de audio según los eventos
/// del sistema. Delega la reproducción física a IAudioDevice (Low Coupling).
/// </summary>
public class SoundController : IEventHandler
{
    public SoundTracks CurrentTrack { get; private set; }
    private IAudioDevice _audioDevice;

    public SoundController() { }

    /// <summary>Ajusta el volumen global del dispositivo de audio.</summary>
    public void SetGlobalVolume(double volume) { }

    /// <summary>Reproduce la pista indicada.</summary>
    public void Play(SoundTracks track) { }

    /// <summary>Detiene la pista indicada.</summary>
    public void Stop(SoundTracks track) { }

    /// <inheritdoc/>
    public void Handle(IEvent @event) { }
}
