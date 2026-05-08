using StadiumSystem.Audio;
using StadiumSystem.Domain.Enums;
using StadiumSystem.Domain.Events;

namespace StadiumSystem.Controllers;


public class SoundController : IEventHandler
{
    public SoundTracks CurrentTrack { get; private set; }
    private IAudioDevice _audioDevice;

    public SoundController() { }

    public void SetGlobalVolume(double volume) { }

    public void Play(SoundTracks track) { }

    public void Stop(SoundTracks track) { }

    public void Handle(IEvent @event) { }
}
