
using Leaderboard.API.Infrastructure.Models.Dtos;

namespace Leaderboard.API.Infrastructure.Models.Dtos
{

  public class GetUserHighestScoreRequest
  {
    public string UserId { get; set; }

    public bool IsValid()
    {
      if (string.IsNullOrEmpty(UserId))
      {
        return false;
      }
      return true;
    }
  }
  public class GetUserHighestScoreResponse
  {
    public string UserId { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalGamesPlayed { get; set; }
    public List<ScoreHistoryDto> RecentScores { get; set; } = new List<ScoreHistoryDto>();
  }
}
