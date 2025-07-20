namespace Leaderboard.API.Infrastructure.Models.Auth
{
    public class TokenResponse
    {
        public string Token { get; set; }
        public long ExpiresAt { get; set; }
    }
}
