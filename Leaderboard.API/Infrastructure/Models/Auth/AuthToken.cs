using System;

namespace Leaderboard.API.Infrastructure.Models.Auth
{
    public class AuthToken
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string UserId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
