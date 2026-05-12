namespace StadiumSystem.Commands;

public class LcdShowScoresCommand : ICommand
{
    public int ScoreLocal { get; }
    public int ScoreAway { get; }

    public LcdShowScoresCommand(int scoreLocal, int scoreAway)
    {
        ScoreLocal = scoreLocal;
        ScoreAway = scoreAway;
    }

    public string Serialize() => $"LCD_SCORES:{ScoreLocal}:{ScoreAway}";
}
