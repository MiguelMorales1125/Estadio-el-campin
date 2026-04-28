using StadiumSystem.Domain;
using StadiumSystem.Events;

namespace StadiumSystem.Controllers;

public class ScoreController : IEventHandler
{
    private Scoreboard _scoreboard;

    public ScoreController()
    {
        _scoreboard = new Scoreboard();
    }

    public void ResetScores() { }

    public void SetScoreboard(Scoreboard scoreboard) { }

    public void IncrementLocalScore() { }

    public void IncrementAwayScore() { }

    public void Handle(IEvent @event) { }
}
