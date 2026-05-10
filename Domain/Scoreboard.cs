using StadiumSystem.Devices.Actuators;

namespace StadiumSystem.Domain;

public class Scoreboard
{
    private int _scoreLocal;
    private int _scoreAway;
    private IScoreDisplay? _display;

    public void SetScore(string team, int newScore)
    {
        if (team.Equals("local", StringComparison.OrdinalIgnoreCase))
            _scoreLocal = newScore;
        else if (team.Equals("away", StringComparison.OrdinalIgnoreCase))
            _scoreAway = newScore;
    }

    public (int local, int away) GetScores() => (_scoreLocal, _scoreAway);

    public void SetDisplay(IScoreDisplay display)
    {
        _display = display;
    }

    public void IncreaseLocal() => _scoreLocal++;

    public void IncreaseAway() => _scoreAway++;
}
