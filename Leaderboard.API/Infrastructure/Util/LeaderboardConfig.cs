using System.ComponentModel.DataAnnotations;

namespace Leaderboard.API.Infrastructure.Util;

public class ConfigSettings
{
    private IConfigurationSection configurationSection;

    [Required] public string ConnectionString { get; set; }
    [Required] public string Database { get; set; }
    public AuthSettings AuthSettings { get; set; }
}

public class AuthSettings
{
    private string _secretKey;
    private string _gamePartnerSecret;

    public string SecretKey
    {
        get => Environment.GetEnvironmentVariable("Leaderboard_SECRET_KEY") ?? _secretKey;
        set => _secretKey = value;
    }

    public string GamePartnerSecret
    {
        get => Environment.GetEnvironmentVariable("Leaderboard_GAME_PARTNER_SECRET") ?? _gamePartnerSecret;
        set => _gamePartnerSecret = value;
    }

    private int _tokenExpirationMinutes = 60;

    public int TokenExpirationMinutes
    {
        get
        {
            if (int.TryParse(Environment.GetEnvironmentVariable("Leaderboard_TOKEN_EXPIRATION_MINUTES"), out int minutes))
                return minutes;
            return _tokenExpirationMinutes; // Default value
        }
        set => _tokenExpirationMinutes = value;
    }

    public string Issuer { get; set; }
    public string Audience { get; set; }
}
