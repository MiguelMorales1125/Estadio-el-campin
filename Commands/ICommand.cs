namespace StadiumSystem.Commands;

/// <summary>
/// GRASP - Polymorphism + Protected Variations: abstrae los comandos
/// enviados al Arduino, permitiendo extender sin modificar.
/// </summary>
public interface ICommand
{
    string Name { get; }
    object? Data { get; }
}
