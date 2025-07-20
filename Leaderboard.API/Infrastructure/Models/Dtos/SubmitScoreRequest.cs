
using System.ComponentModel.DataAnnotations;
using Leaderboard.API.Infrastructure.Errors;
using Leaderboard.API.Infrastructure.Exceptions;

namespace Leaderboard.API.Infrastructure.Models.Dtos
{
  public class SubmitScoreRequest
  {
    [Required]
    [StringLength(50)]
    public string Name { get; set; }
    [Required]
    public int Score { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsValid()
    {
      if (string.IsNullOrEmpty(Name))
      {
        return false;
      }
      if (Score < 0)
      {
        return false;
      }
      return true;
    }
  }

  public class SubmitScoreResponse
  {
    public string UserId { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
    public string? ImageUrl { get; set; }
    public int? HighestScore { get; set; }
    public int? TotalGamesPlayed { get; set; }
    public DateTime CreatedAt { get; set; }
  }
}
