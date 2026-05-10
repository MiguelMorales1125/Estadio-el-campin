namespace StadiumSystem.Infrastructure;

public class ArduinoInventoryMessage
{
    public List<LedDeviceInfo> Leds { get; set; } = new();
    public List<SensorDeviceInfo> Sensors { get; set; } = new();
    public List<ActuatorDeviceInfo> Actuators { get; set; } = new();
}

public class LedDeviceInfo
{
    public int Pin { get; set; }
    public string Type { get; set; } = "";
}

public class SensorDeviceInfo
{
    public string Type { get; set; } = "";
    public int Pin { get; set; }
}

public class ActuatorDeviceInfo
{
    public string Type { get; set; } = "";
    public int Pin { get; set; }
}

public class ArduinoInventoryParser
{
    public ArduinoInventoryMessage Parse(string message)
    {
        var inventory = new ArduinoInventoryMessage();

        if (!message.StartsWith("INVENTORY:"))
            return inventory;

        var content = message.Substring("INVENTORY:".Length);
        var parts = content.Split(',');

        foreach (var part in parts)
        {
            var tokens = part.Split(':');
            if (tokens.Length < 3)
                continue;

            var deviceType = tokens[0].Trim();
            var subType = tokens[1].Trim();
            var pin = tokens[2].Trim();

            if (!int.TryParse(pin, out var pinNumber))
                continue;

            switch (deviceType)
            {
                case "LED":
                    inventory.Leds.Add(new LedDeviceInfo { Pin = pinNumber, Type = subType });
                    break;
                case "SENSOR":
                    inventory.Sensors.Add(new SensorDeviceInfo { Type = subType, Pin = pinNumber });
                    break;
                case "ACTUATOR":
                    inventory.Actuators.Add(new ActuatorDeviceInfo { Type = subType, Pin = pinNumber });
                    break;
            }
        }

        return inventory;
    }
}
