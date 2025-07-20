using System.ComponentModel.DataAnnotations;

namespace Leaderboard.API.Infrastructure.Models.Dtos
{
  public class ScoreHistoryDto
  {
    public int Score { get; set; }
    public DateTime Timestamp { get; set; }
    public string? DeviceType { get; set; }
  }
  public class GetLeaderBoardRequest
  {
    public int? Limit { get; set; } = 10;
    public int? Skip { get; set; } = 0;

    public bool IsValid()
    {
      if (Skip < 0 || Limit < 0)
      {
        return false;
      }
      return true;
    }
  }

  public class LeaderboardFilterRequest
  {
    [Required]
    public int? Limit { get; set; } = 10;
    public int? Skip { get; set; } = 0;
    public bool? Ascending { get; set; } = false;
    public List<string>? UserIds { get; set; }
    public string? DeviceType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MinScore { get; set; }
    public int? MaxScore { get; set; }

    public bool IsValid()
    {
      if (Skip < 0 || Limit < 0)
      {
        return false;
      }

      if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
      {
        return false;
      }

      if (MinScore.HasValue && MaxScore.HasValue && MinScore > MaxScore)
      {
        return false;
      }

      return true;
    }
  }

  public class LeaderboardEntryDto
  {
    public string UserId { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
    public string? ImageUrl { get; set; }
    public string? DeviceType { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalGamesPlayed { get; set; }
    public List<ScoreHistoryDto> RecentScores { get; set; } = new List<ScoreHistoryDto>();
  }

  public class GetLeaderBoardResponse
  {
    public List<LeaderboardEntryDto> Entries { get; set; } = new List<LeaderboardEntryDto>();
    public int TotalCount { get; set; }
  }
}
