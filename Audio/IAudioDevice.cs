using StadiumSystem.Domain.Enums;

namespace StadiumSystem.Audio;

public interface IAudioDevice
{
    void Play(SoundTracks track);
    void Stop(SoundTracks track);
    void SetVolume(double volume);
}
