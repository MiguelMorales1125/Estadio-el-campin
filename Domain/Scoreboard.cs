using StadiumSystem.Devices.Actuators;

namespace StadiumSystem.Domain;

public class Scoreboard
{
    private int _scoreLocal;
    private int _scoreAway;
    private IScoreDisplay _display;

    public void SetScore(string team, int newScore) { }

    public (int local, int away) GetScores() { return (_scoreLocal, _scoreAway); }

    public void SetDisplay(IScoreDisplay display) { }
}
