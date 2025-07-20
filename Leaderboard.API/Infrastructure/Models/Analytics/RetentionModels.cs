using System;
using System.Collections.Generic;

namespace Leaderboard.API.Infrastructure.Models.Analytics
{
    public class GetRetentionRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetRetentionRatesResponse
    {
        public int TotalPlayers { get; set; }
        public RetentionMetrics DailyRetention { get; set; }
        public RetentionMetrics ThreeDayRetention { get; set; }
        public RetentionMetrics WeeklyRetention { get; set; }
        public RetentionMetrics MonthlyRetention { get; set; }
        public NewVsReturningMetrics NewVsReturningPlayers { get; set; }
    }

    public class RetentionMetrics
    {
        public int EligiblePlayers { get; set; }
        public int ReturnedPlayers { get; set; }
        public double RetentionRate { get; set; }
    }

    public class NewVsReturningMetrics
    {
        public int NewPlayers { get; set; }
        public int ReturningPlayers { get; set; }
        public double NewPlayersPercentage { get; set; }
        public double ReturningPlayersPercentage { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class PlayerActivityData
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public DateTime FirstPlayed { get; set; }
        public List<DateTime> PlayDates { get; set; } = new List<DateTime>();
    }
}
