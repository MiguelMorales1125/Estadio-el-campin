using StadiumSystem.Devices.Actuators;

namespace StadiumSystem.Domain;

/// <summary>
/// GRASP - Information Expert: agrupa un LED y conoce su estado lógico
/// como "luz del estadio" (encendido, apagado).
/// </summary>
public class Light
{
    private LED _led;

    public void On() { }
    public void Off() { }
    public bool IsOn() { return false; }
    public void SetLed(LED led) { }
}
