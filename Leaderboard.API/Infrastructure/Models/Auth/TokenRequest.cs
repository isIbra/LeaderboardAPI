namespace Leaderboard.API.Infrastructure.Models.Auth
{
    public class TokenRequest
    {
        public string UserId { get; set; }
        public string Signature { get; set; }
        public long Timestamp { get; set; }
    }
}
