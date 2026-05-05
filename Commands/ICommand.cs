namespace StadiumSystem.Commands;

public interface ICommand
{
    string Name { get; }
    object? Data { get; }
}
