using System;

namespace Leaderboard.API.Infrastructure.Models.Dtos
{
    public class GetUserStatsResponse
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public int? HighestScore { get; set; }
        public int? TotalGamesPlayed { get; set; }
        public DateTime? LastPlayed { get; set; }
        public bool IsNewUser { get; set; }
        public List<ScoreHistoryDto> RecentScores { get; set; } = new List<ScoreHistoryDto>();
    }
}
