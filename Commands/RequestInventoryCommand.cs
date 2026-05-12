namespace StadiumSystem.Commands;

public class RequestInventoryCommand : ICommand
{
    public string Serialize() => "REQUEST_INVENTORY";
}
