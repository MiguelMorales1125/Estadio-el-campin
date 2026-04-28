namespace StadiumSystem.Commands;

<summary>
</summary>
public interface ICommand
{
    string Name { get; }
    object? Data { get; }
}
