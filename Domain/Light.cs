using StadiumSystem.Devices.Actuators;

namespace StadiumSystem.Domain;

public class Light
{
    private LED? _led;

    public void On()
    {
        _led?.On();
    }

    public void Off()
    {
        _led?.Off();
    }

    public bool IsOn() => _led?.IsOn ?? false;

    public void SetLed(LED led)
    {
        _led = led;
    }

    public async Task BlinkAsync(int times = 1, int delayMs = 500)
    {
        if (_led != null)
            await _led.BlinkAsync(times, delayMs);
    }
}
