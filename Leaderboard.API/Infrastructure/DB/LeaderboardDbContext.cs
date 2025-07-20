using Leaderboard.API.Infrastructure.Models.Shared;
using Leaderboard.API.Infrastructure.Util;
using Leaderboard.API.Infrastructure.Models.Auth;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Leaderboard.API.Infrastructure.Models;

namespace Leaderboard.API.Infrastructure.DbContext
{

  public class LeaderboardDbContext
  {
    private readonly IMongoDatabase _database;

    public LeaderboardDbContext(IOptions<ConfigSettings> settings)
    {
      var client = new MongoClient(settings.Value.ConnectionString);

      if (client == null)
      {
        throw new MongoClientException("Could not connect to Mongo");
      }

      _database = client.GetDatabase(settings.Value.Database);

      // CreateIndexes();
    }

    public IMongoCollection<LeaderboardEntry> leaderboards => _database.GetCollection<LeaderboardEntry>("UserLeaderboard");
    public IMongoCollection<AuthToken> authTokens => _database.GetCollection<AuthToken>("AuthTokens");
    public void CreateIndexes()
    {
      CreateIndexOptions indexOptions = new CreateIndexOptions { Background = true };
      CreateIndexOptions uniqueIndexOptions = new CreateIndexOptions { Background = true, Unique = true };

      // Leaderboard Indexes
      leaderboards.Indexes.CreateOne(new CreateIndexModel<LeaderboardEntry>(
          Builders<LeaderboardEntry>.IndexKeys.Ascending(x => x.UserId), uniqueIndexOptions));
      leaderboards.Indexes.CreateOne(new CreateIndexModel<LeaderboardEntry>(Builders<LeaderboardEntry>.IndexKeys.Descending(x => x.BestScore), indexOptions));
      leaderboards.Indexes.CreateOne(new CreateIndexModel<LeaderboardEntry>(Builders<LeaderboardEntry>.IndexKeys.Descending(x => x.LastUpdated), indexOptions));

      // AuthToken Indexes
      authTokens.Indexes.CreateOne(new CreateIndexModel<AuthToken>(Builders<AuthToken>.IndexKeys.Ascending(x => x.Token), uniqueIndexOptions));
      authTokens.Indexes.CreateOne(new CreateIndexModel<AuthToken>(Builders<AuthToken>.IndexKeys.Ascending(x => x.UserId), indexOptions));
      authTokens.Indexes.CreateOne(new CreateIndexModel<AuthToken>(Builders<AuthToken>.IndexKeys.Ascending(x => x.ExpiresAt), indexOptions));

    }
  }

}
