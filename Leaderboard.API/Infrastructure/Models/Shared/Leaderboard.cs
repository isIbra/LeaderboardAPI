
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Leaderboard.API.Infrastructure.Models.Shared
{
  public class LeaderboardEntry
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Username { get; set; }
    public int BestScore { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string? ImageUrl { get; set; }
    public List<ScoreEntry> RecentScores { get; set; } = new List<ScoreEntry>();
    public int TotalGamesPlayed { get; set; }
  }


  public class ScoreEntry
  {
    public int Score { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? DeviceType { get; set; }
  }
}
