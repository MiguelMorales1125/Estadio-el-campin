namespace StadiumSystem.Commands;

public class ScoreUpdateCommand : ICommand
{
    public int ScoreLocal { get; }
    public int ScoreAway { get; }

    public ScoreUpdateCommand(int scoreLocal, int scoreAway)
    {
        ScoreLocal = scoreLocal;
        ScoreAway = scoreAway;
    }

    public string Serialize() => $"SCORE_UPDATE:{ScoreLocal}:{ScoreAway}";
}
