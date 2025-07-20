using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Leaderboard.API.Infrastructure.DbContext;
using Leaderboard.API.Infrastructure.Models.Analytics;
using Leaderboard.API.Infrastructure.Models.Shared;
using MongoDB.Driver;

namespace Leaderboard.API.Infrastructure.Repositories
{
    public interface IRetentionRepository
    {
        Task<List<LeaderboardEntry>> GetLeaderboardDataAsync(
            DateTime? startDate,
            DateTime? endDate);
    }

    public class RetentionRepository : IRetentionRepository
    {
        private readonly LeaderboardDbContext _context;

        public RetentionRepository(LeaderboardDbContext context)
        {
            _context = context;
        }

        public async Task<List<LeaderboardEntry>> GetLeaderboardDataAsync(
            DateTime? startDate,
            DateTime? endDate)
        {
            var filterBuilder = Builders<LeaderboardEntry>.Filter;
            var filter = FilterDefinition<LeaderboardEntry>.Empty;

            if (endDate.HasValue)
            {
                filter = filter & filterBuilder.Lte(x => x.LastUpdated, endDate.Value);
            }


            // Return all user leaderboard data in a single query
            return await _context.leaderboards
                .Find(filter)
                .ToListAsync();
        }
    }
}
