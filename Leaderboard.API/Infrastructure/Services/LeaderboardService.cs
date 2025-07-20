using System.Security.Cryptography.X509Certificates;
using Leaderboard.API.Infrastructure.Errors;
using Leaderboard.API.Infrastructure.Exceptions;
using Leaderboard.API.Infrastructure.Models;
using Leaderboard.API.Infrastructure.Models.Dtos;
using Leaderboard.API.Infrastructure.Repositories;
using Leaderboard.API.Infrastructure.Models.Shared;
using Serilog;

namespace Leaderboard.API.Infrastructure.Services
{
    public interface ILeaderboardService
    {
        Task<GetLeaderBoardResponse> GetLeaderboardAsync(GetLeaderBoardRequest request);
        Task<GetLeaderBoardResponse> GetLeaderboardFilteredAsync(LeaderboardFilterRequest request);
        Task<GetUserHighestScoreResponse> GetUserHighestScoreAsync(GetUserHighestScoreRequest request);
        Task<SubmitScoreResponse> SubmitScore(SubmitScoreRequest request, string userId, string DeviceType);
        Task<GetUserStatsResponse> GetUserStatsAsync(string userId);
    }

    public class LeaderboardService : ILeaderboardService
    {
        private readonly ILeaderboardRepository _leaderboardRepository;

        public LeaderboardService(ILeaderboardRepository leaderboardRepository)
        {
            _leaderboardRepository = leaderboardRepository;
        }

        public async Task<GetLeaderBoardResponse> GetLeaderboardAsync(GetLeaderBoardRequest request)
        {
            try
            {
                Log.Information("Getting leaderboard with request: {@Request}", request);
                (List<LeaderboardEntry> leaderboards, int totalCount) = await _leaderboardRepository.GetLeaderboardAsync(request);

                GetLeaderBoardResponse response = new GetLeaderBoardResponse
                {
                    TotalCount = totalCount,
                    Entries = leaderboards.Select(MapToLeaderboardEntryDto).ToList()
                };

                return response;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting leaderboard");
                throw new Exception("An error occurred while retrieving the leaderboard", ex);
            }
        }

        public async Task<GetLeaderBoardResponse> GetLeaderboardFilteredAsync(LeaderboardFilterRequest request)
        {
            try
            {
                Log.Information("Getting filtered leaderboard with request: {@Request}", request);
                (List<LeaderboardEntry> leaderboards, int totalCount) = await _leaderboardRepository.GetLeaderboardFilteredAsync(request);

                GetLeaderBoardResponse response = new GetLeaderBoardResponse
                {
                    TotalCount = totalCount,
                    Entries = leaderboards.Select(MapToLeaderboardEntryDto).ToList()
                };

                return response;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting filtered leaderboard");
                throw new Exception("An error occurred while retrieving the filtered leaderboard", ex);
            }
        }

        public async Task<GetUserHighestScoreResponse> GetUserHighestScoreAsync(GetUserHighestScoreRequest request)
        {
            try
            {
                Log.Information("Getting user highest score for user {UserId}", request.UserId);
                LeaderboardEntry userLeaderboard = await _leaderboardRepository.GetUserLeaderboard(request.UserId);

                if (userLeaderboard == null)
                {
                    return new GetUserHighestScoreResponse();
                }

                GetUserHighestScoreResponse getUserHighestScoreResponse = new GetUserHighestScoreResponse
                {
                    UserId = userLeaderboard.UserId,
                    Name = userLeaderboard.Username,
                    Score = userLeaderboard.BestScore,
                    CreatedAt = userLeaderboard.LastUpdated,
                    TotalGamesPlayed = userLeaderboard.TotalGamesPlayed,
                    RecentScores = userLeaderboard.RecentScores?.Select(x => new ScoreHistoryDto
                    {
                        Score = x.Score,
                        Timestamp = x.Timestamp,
                        DeviceType = x.DeviceType
                    }).ToList() ?? new List<ScoreHistoryDto>()
                };

                return getUserHighestScoreResponse;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting user highest score");
                throw new Exception("An error occurred while retrieving the user's highest score", ex);
            }
        }

        public async Task<SubmitScoreResponse> SubmitScore(SubmitScoreRequest request, string userId, string deviceType)
        {
            Log.Information("Submitting score for user {UserId} with request: {@Request}", userId, request);
            UserScores userScores = new UserScores();
            userScores.UserId = userId;
            userScores.Name = request.Name;
            userScores.Score = request.Score;
            userScores.ImageUrl = request.ImageUrl;
            userScores.DeviceType = deviceType;

            if (!ValidateScore(userScores.Score))
                throw new ApiException(ErrorResponses.InvalidScore);

            try
            {
                LeaderboardEntry result = await _leaderboardRepository.SubmitScore(userScores);

                Log.Information("userScores: {@UserScores}", result);

                SubmitScoreResponse response = new SubmitScoreResponse { };
                response.UserId = result.UserId;
                response.Name = result.Username;
                response.Score = userScores.Score;
                response.ImageUrl = result.ImageUrl;
                response.HighestScore = result.BestScore;
                response.TotalGamesPlayed = result.TotalGamesPlayed;
                response.CreatedAt = result.LastUpdated;

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while submitting the score", ex);
            }
        }

        private LeaderboardEntryDto MapToLeaderboardEntryDto(LeaderboardEntry leaderboard)
        {
            return new LeaderboardEntryDto
            {
                UserId = leaderboard.UserId,
                Name = leaderboard.Username,
                Score = leaderboard.BestScore,
                ImageUrl = leaderboard.ImageUrl,
                DeviceType = leaderboard.RecentScores?.FirstOrDefault(x => x.DeviceType != null)?.DeviceType,
                CreatedAt = leaderboard.LastUpdated,
                TotalGamesPlayed = leaderboard.TotalGamesPlayed,

                RecentScores = leaderboard.RecentScores?.Select(x => new ScoreHistoryDto
                {
                    Score = x.Score,
                    Timestamp = x.Timestamp,
                    DeviceType = x.DeviceType
                }).ToList() ?? new List<ScoreHistoryDto>()
            };
        }

        private bool ValidateScore(int score)
        {
            // we can keep adding rules here
            if (score < Rules.MinScore || score > Rules.MaxScore)
            {
                Log.Information("Score is out of range: {Score} with validation : {Validation}", score, score < Rules.MinScore || score > Rules.MaxScore);
                return false;
            }

            if (score < 0)
            {
                Log.Information("Score is negative: {Score} with validation : {Validation}", score, score < 0);
                return false;
            }

            if (score % 1 != 0)
            {
                Log.Information("Score is not an integer: {Score} with validation : {Validation}", score, score % 1 != 0);
                return false;
            }

            return true;
        }

        public async Task<GetUserStatsResponse> GetUserStatsAsync(string userId)
        {
            try
            {
                Log.Information("Getting user stats for user {UserId}", userId);

                LeaderboardEntry? userLeaderboard = await _leaderboardRepository.GetUserLeaderboard(userId);

                bool isNewUser = (userLeaderboard == null);

                int? highScore = userLeaderboard?.BestScore;
                int totalGames = userLeaderboard?.TotalGamesPlayed ?? 0;

                var response = new GetUserStatsResponse
                {
                    UserId = userId,
                    Name = userLeaderboard?.Username,
                    ImageUrl = userLeaderboard?.ImageUrl,
                    HighestScore = highScore,
                    TotalGamesPlayed = totalGames,
                    LastPlayed = userLeaderboard?.LastUpdated,
                    IsNewUser = isNewUser,
                    RecentScores = userLeaderboard?.RecentScores?.Select(x => new ScoreHistoryDto
                    {
                        Score = x.Score,
                        Timestamp = x.Timestamp,
                        DeviceType = x.DeviceType
                    }).ToList() ?? new List<ScoreHistoryDto>()
                };

                return response;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting user stats");
                throw new Exception("An error occurred while retrieving the user stats", ex);
            }
        }

    }
}
