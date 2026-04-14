using StadiumSystem.Enums;

namespace StadiumSystem.Domain;

/// <summary>
/// GRASP - Information Expert: representa el estadio físico; conoce su
/// estado actual, sus luces y su marcador. Actúa como raíz del modelo.
/// </summary>
public class Stadium
{
    private StadiumStates _actualState;
    private List<Light> _lights;
    private Scoreboard _scoreboard;

    public Stadium()
    {
        _lights = new List<Light>();
        _scoreboard = new Scoreboard();
    }

    public void ChangeState(StadiumStates state) { }
    public List<Light> GetLights() { return _lights; }
    public Scoreboard GetScoreboard() { return _scoreboard; }
    public StadiumStates GetState() { return _actualState; }

    // Getters & setters adicionales según necesidad
}
