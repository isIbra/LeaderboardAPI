using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Leaderboard.API.Infrastructure.Models.Auth;
using Leaderboard.API.Infrastructure.Util;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Leaderboard.API.Infrastructure.Services
{
    public interface IAuthService
    {
        Task<TokenResponse> GenerateTokenAsync(TokenRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly ConfigSettings _settings;

        public AuthService(IOptions<ConfigSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<TokenResponse> GenerateTokenAsync(TokenRequest request)
        {
            bool isValidSignature = await VerifySignatureAsync(request);
            if (!isValidSignature)
            {
                throw new UnauthorizedAccessException("Invalid signature");
            }

            var (tokenString, expiration) = CreateToken(request.UserId);

            return new TokenResponse
            {
                Token = tokenString,
                ExpiresAt = new DateTimeOffset(expiration).ToUnixTimeSeconds()
            };
        }


        private async Task<bool> VerifySignatureAsync(TokenRequest request)
        {
            var requestTime = DateTimeOffset.FromUnixTimeMilliseconds(request.Timestamp).UtcDateTime;
            if (DateTime.UtcNow.Subtract(requestTime).TotalMinutes > 5)
            {
                return false;
            }

            // Recreate the signature for verification
            var dataToSign = $"{request.UserId}:{request.Timestamp}";
            var secretBytes = Encoding.UTF8.GetBytes(_settings.AuthSettings.GamePartnerSecret);

            using (var hmac = new HMACSHA256(secretBytes))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
                var computedSignature = Convert.ToBase64String(computedHash);

                return computedSignature == request.Signature;
            }
        }

        private (string tokenString, DateTime expiration) CreateToken(string userId)
        {
            var expiration = DateTime.UtcNow.AddMinutes(_settings.AuthSettings.TokenExpirationMinutes);

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_settings.AuthSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim("userId", userId),
                }),
                Expires = expiration,
                Issuer = _settings.AuthSettings.Issuer,
                Audience = _settings.AuthSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(securityToken);

            return (tokenString, expiration);
        }
    }
}
