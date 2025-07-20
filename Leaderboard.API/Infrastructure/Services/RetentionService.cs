using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Leaderboard.API.Infrastructure.Models.Analytics;
using Leaderboard.API.Infrastructure.Repositories;
using Leaderboard.API.Infrastructure.Models.Shared;
using Serilog;

namespace Leaderboard.API.Infrastructure.Services
{
    public interface IRetentionService
    {
        Task<GetRetentionRatesResponse> GetRetentionRatesAsync(GetRetentionRequest request);
        Task<NewVsReturningMetrics> GetNewVsReturningMetricsAsync(GetRetentionRequest request);
    }

    public class RetentionService : IRetentionService
    {
        private readonly IRetentionRepository _retentionRepository;

        public RetentionService(IRetentionRepository retentionRepository)
        {
            _retentionRepository = retentionRepository;
        }

        public async Task<GetRetentionRatesResponse> GetRetentionRatesAsync(GetRetentionRequest request)
        {
            try
            {
                Log.Information("Getting retention rates with request: {@Request}", request);

                DateTime endDate = request.EndDate ?? DateTime.UtcNow;
                DateTime startDate = request.StartDate ?? endDate.AddDays(-30);


                List<LeaderboardEntry>? leaderboardData = await _retentionRepository.GetLeaderboardDataAsync(null, endDate);

                List<PlayerActivityData>? playerActivityData = ProcessPlayerActivityData(leaderboardData, startDate, endDate);

                int totalPlayers = playerActivityData.Count;

                RetentionMetrics dailyRetention = CalculateRetention(playerActivityData, 1, startDate, endDate);
                RetentionMetrics threeDayRetention = CalculateRetention(playerActivityData, 3, startDate, endDate);
                RetentionMetrics weeklyRetention = CalculateRetention(playerActivityData, 7, startDate, endDate);
                RetentionMetrics monthlyRetention = CalculateRetention(playerActivityData, 30, startDate, endDate);

                var newVsReturning = CalculateNewVsReturningPlayers(playerActivityData, startDate, endDate);

                return new GetRetentionRatesResponse
                {
                    TotalPlayers = totalPlayers,
                    DailyRetention = dailyRetention,
                    ThreeDayRetention = threeDayRetention,
                    WeeklyRetention = weeklyRetention,
                    MonthlyRetention = monthlyRetention,
                    NewVsReturningPlayers = newVsReturning
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error calculating retention rates");
                throw new Exception("An error occurred while calculating retention rates", ex);
            }
        }

        public async Task<NewVsReturningMetrics> GetNewVsReturningMetricsAsync(GetRetentionRequest request)
        {
            try
            {
                Log.Information("Getting new vs returning metrics with request: {@Request}", request);

                DateTime endDate = request.EndDate ?? DateTime.UtcNow;
                DateTime startDate = request.StartDate ?? endDate.AddDays(-30);


                var leaderboardData = await _retentionRepository.GetLeaderboardDataAsync(null, endDate);

                var playerActivityData = ProcessPlayerActivityData(leaderboardData, startDate, endDate);

                return CalculateNewVsReturningPlayers(playerActivityData, startDate, endDate);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error calculating new vs returning player metrics");
                throw new Exception("An error occurred while calculating new vs returning player metrics", ex);
            }
        }

        private List<PlayerActivityData> ProcessPlayerActivityData(
            List<LeaderboardEntry> leaderboards,
            DateTime startDate,
            DateTime endDate)
        {
            IEnumerable<IGrouping<string, LeaderboardEntry>>? userGroups = leaderboards.GroupBy(l => l.UserId);

            List<PlayerActivityData>? playerData = new List<PlayerActivityData>();

            foreach (IGrouping<string, LeaderboardEntry> userGroup in userGroups)
            {
                PlayerActivityData? userData = new PlayerActivityData
                {
                    UserId = userGroup.Key,
                    Username = userGroup.First().Username,
                    FirstPlayed = GetFirstPlayDate(userGroup.ToList())
                };

                foreach (LeaderboardEntry leaderboard in userGroup)
                {
                    if (leaderboard.RecentScores != null)
                    {
                        userData.PlayDates.AddRange(
                            leaderboard.RecentScores
                                .Select(s => s.Timestamp)
                                .Distinct()
                        );
                    }

                    if (!userData.PlayDates.Contains(leaderboard.LastUpdated))
                    {
                        userData.PlayDates.Add(leaderboard.LastUpdated);
                    }
                }

                userData.PlayDates = userData.PlayDates
                    .OrderBy(d => d)
                    .ToList();

                playerData.Add(userData);
            }

            return playerData;
        }

        private DateTime GetFirstPlayDate(List<LeaderboardEntry> userLeaderboards)
        {
            var earliestFromScores = userLeaderboards
                .Where(l => l.RecentScores != null && l.RecentScores.Any())
                .SelectMany(l => l.RecentScores.Select(s => s.Timestamp))
                .DefaultIfEmpty(DateTime.MaxValue)
                .Min();

            var earliestLastUpdated = userLeaderboards
                .Min(l => l.LastUpdated);

            return earliestFromScores < earliestLastUpdated ?
                earliestFromScores :
                earliestLastUpdated;
        }

        private RetentionMetrics CalculateRetention(
            List<PlayerActivityData> playerData,
            int days,
            DateTime startDate,
            DateTime endDate)
        {
            var cutoffDate = endDate.AddDays(-days);

            List<PlayerActivityData>? eligiblePlayers = playerData
                .Where(p => p.PlayDates.Any(d => d <= cutoffDate))
                .ToList();

            List<PlayerActivityData>? returnedPlayers = eligiblePlayers
                .Where(p => p.PlayDates.Any(d => d > cutoffDate && d <= endDate))
                .ToList();

            double retentionRate = eligiblePlayers.Count > 0
                ? (double)returnedPlayers.Count / eligiblePlayers.Count * 100
                : 0;

            return new RetentionMetrics
            {
                EligiblePlayers = eligiblePlayers.Count,
                ReturnedPlayers = returnedPlayers.Count,
                RetentionRate = Math.Round(retentionRate, 2)
            };
        }

        private NewVsReturningMetrics CalculateNewVsReturningPlayers(
            List<PlayerActivityData> playerData,
            DateTime startDate,
            DateTime endDate)
        {
            List<PlayerActivityData>? activePlayers = playerData
                .Where(p => p.PlayDates.Any(d => d >= startDate && d <= endDate))
                .ToList();

            int newPlayers = activePlayers
                .Count(p => p.FirstPlayed >= startDate && p.FirstPlayed <= endDate);

            int returningPlayers = activePlayers
                .Count(p => p.FirstPlayed < startDate);

            int totalActivePlayers = newPlayers + returningPlayers;

            double newPercentage = totalActivePlayers > 0
                ? (double)newPlayers / totalActivePlayers * 100
                : 0;

            double returningPercentage = totalActivePlayers > 0
                ? (double)returningPlayers / totalActivePlayers * 100
                : 0;

            return new NewVsReturningMetrics
            {
                NewPlayers = newPlayers,
                ReturningPlayers = returningPlayers,
                NewPlayersPercentage = Math.Round(newPercentage, 2),
                ReturningPlayersPercentage = Math.Round(returningPercentage, 2),
                PeriodStart = startDate,
                PeriodEnd = endDate
            };
        }
    }
}
