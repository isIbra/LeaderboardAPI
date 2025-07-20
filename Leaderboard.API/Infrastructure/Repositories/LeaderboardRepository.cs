using Leaderboard.API.Infrastructure.DbContext;
using Leaderboard.API.Infrastructure.Models;
using Leaderboard.API.Infrastructure.Models.Dtos;
using Leaderboard.API.Infrastructure.Models.Shared;
using MongoDB.Driver;
using System.Security.Cryptography.X509Certificates;

namespace Leaderboard.API.Infrastructure.Repositories
{
    public interface ILeaderboardRepository
    {
        Task<(List<LeaderboardEntry>, int)> GetLeaderboardAsync(GetLeaderBoardRequest request);
        Task<(List<LeaderboardEntry>, int)> GetLeaderboardFilteredAsync(Models.Dtos.LeaderboardFilterRequest request);
        Task<LeaderboardEntry> GetUserLeaderboard(string userId);
        Task<LeaderboardEntry> SubmitScore(UserScores scoreEntry);
    }

    public class LeaderboardRepository : ILeaderboardRepository
    {
        private readonly LeaderboardDbContext _context;
        private const int MAX_RECENT_SCORES = 10; // Limit recent scores array size for performance balance

        public LeaderboardRepository(LeaderboardDbContext context)
        {
            _context = context;
        }

        public async Task<(List<LeaderboardEntry>, int)> GetLeaderboardAsync(GetLeaderBoardRequest request)
        {
            var filterBuilder = Builders<LeaderboardEntry>.Filter;
            var filter = FilterDefinition<LeaderboardEntry>.Empty;


            var scores = await _context.leaderboards
                .Find(filter)
                .Sort(Builders<LeaderboardEntry>.Sort.Descending(x => x.BestScore))
                .Skip(request.Skip)
                .Limit(request.Limit)
                .ToListAsync();

            long totalCount = await _context.leaderboards.CountDocumentsAsync(filter);

            return (scores, (int)totalCount);
        }

        public async Task<(List<LeaderboardEntry>, int)> GetLeaderboardFilteredAsync(Models.Dtos.LeaderboardFilterRequest request)
        {
            var filterBuilder = Builders<LeaderboardEntry>.Filter;
            var filter = FilterDefinition<LeaderboardEntry>.Empty;


            // Apply user IDs filter if provided
            if (request.UserIds != null && request.UserIds.Count > 0)
            {
                filter = filter & filterBuilder.In(x => x.UserId, request.UserIds);
            }

            // Apply date range filter if provided
            if (request.StartDate.HasValue)
            {
                filter = filter & filterBuilder.Gte(x => x.LastUpdated, request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                filter = filter & filterBuilder.Lte(x => x.LastUpdated, request.EndDate.Value);
            }

            // Apply score range filter if provided
            if (request.MinScore.HasValue)
            {
                filter = filter & filterBuilder.Gte(x => x.BestScore, request.MinScore.Value);
            }

            if (request.MaxScore.HasValue)
            {
                filter = filter & filterBuilder.Lte(x => x.BestScore, request.MaxScore.Value);
            }

            // Apply sort direction based on ascending flag
            var sortDefinition = request.Ascending ?? false
                ? Builders<LeaderboardEntry>.Sort.Ascending(x => x.BestScore)
                : Builders<LeaderboardEntry>.Sort.Descending(x => x.BestScore);

            var scores = await _context.leaderboards
                .Find(filter)
                .Sort(sortDefinition)
                .Skip(request.Skip ?? 0)
                .Limit(request.Limit ?? 10)
                .ToListAsync();

            long totalCount = await _context.leaderboards.CountDocumentsAsync(filter);

            return (scores, (int)totalCount);
        }

        public async Task<LeaderboardEntry> GetUserLeaderboard(string userId)
        {
            var filterBuilder = Builders<LeaderboardEntry>.Filter;
            var filter = filterBuilder.Eq(x => x.UserId, userId);

            return await _context.leaderboards
                .Find(filter)
                .FirstOrDefaultAsync();
        }


        public async Task<LeaderboardEntry> SubmitScore(UserScores scoreEntry)
        {
            // Find or create user leaderboard document
            var filterBuilder = Builders<LeaderboardEntry>.Filter;
            var filter = filterBuilder.Eq(x => x.UserId, scoreEntry.UserId);

            var userLeaderboard = await _context.leaderboards
                .Find(filter)
                .FirstOrDefaultAsync();

            if (userLeaderboard == null)
            {
                // Create new leaderboard entry for this user and game type
                userLeaderboard = new LeaderboardEntry {};
                    userLeaderboard.UserId = scoreEntry.UserId;
                    userLeaderboard.Username = scoreEntry.Name;
                    userLeaderboard.BestScore = scoreEntry.Score;
                    userLeaderboard.LastUpdated = DateTime.UtcNow;
                    userLeaderboard.ImageUrl = scoreEntry.ImageUrl;
                    userLeaderboard.TotalGamesPlayed = 1;
                    userLeaderboard.RecentScores = new List<ScoreEntry> {};
                    userLeaderboard.RecentScores.Add(new ScoreEntry
                    {
                        Score = scoreEntry.Score,
                        Timestamp = DateTime.UtcNow,
                        DeviceType = scoreEntry.DeviceType
                    });


                await _context.leaderboards.InsertOneAsync(userLeaderboard);
            }
            else
            {
                bool isBestScore = scoreEntry.Score > userLeaderboard.BestScore;

                var newScore = new ScoreEntry
                {
                    Score = scoreEntry.Score,
                    Timestamp = DateTime.UtcNow,
                    DeviceType = scoreEntry.DeviceType
                };

                var recentScores = userLeaderboard.RecentScores ?? new List<ScoreEntry>();
                recentScores.Insert(0, newScore);

                if (recentScores.Count > MAX_RECENT_SCORES)
                {
                    recentScores = recentScores.Take(MAX_RECENT_SCORES).ToList();
                }

                var update = Builders<LeaderboardEntry>.Update
                    .Set(x => x.LastUpdated, DateTime.UtcNow)
                    .Set(x => x.RecentScores, recentScores)
                    .Inc(x => x.TotalGamesPlayed, 1);


                if (isBestScore)
                {
                    update = update.Set(x => x.BestScore, scoreEntry.Score);
                }

                await _context.leaderboards.UpdateOneAsync(filter, update);

                FilterDefinition<LeaderboardEntry>? fetchFilter = filterBuilder.Eq(x => x.UserId, scoreEntry.UserId);

                userLeaderboard = await _context.leaderboards
                    .Find(fetchFilter)
                    .FirstOrDefaultAsync();
            }

            return userLeaderboard;
        }

    }
}
