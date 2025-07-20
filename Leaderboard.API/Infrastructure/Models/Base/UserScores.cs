using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Leaderboard.API.Infrastructure.Models
{
    public class UserScores
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public string? DeviceType { get; set; }
        public string? ImageUrl { get; set; }
        public int HighestScore { get; set; }
        public int TotalGamesPlayed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
