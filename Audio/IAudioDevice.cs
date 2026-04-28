using StadiumSystem.Enums;

namespace StadiumSystem.Audio;

<summary>

</summary>
public interface IAudioDevice
{
    void Play(SoundTracks track);
    void Stop(SoundTracks track);
    void SetVolume(double volume);
}
