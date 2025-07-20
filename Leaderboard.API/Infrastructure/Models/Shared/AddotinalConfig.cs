namespace Leaderboard.API.Infrastructure.Models.Shared
{
  public class AddtionalData
  {
    public int HighestScoreInfinite { get; set; }
    public int HighestScoreTimed { get; set; }
    public int GameEntriesInfinite { get; set; }
    public int GameEntriesTimed { get; set; }
    public int TotalGamesPlayed { get; set; }
  }
}
