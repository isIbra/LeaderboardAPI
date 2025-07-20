using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Leaderboard.API.Models;
using Leaderboard.API.Infrastructure.Services;
using Leaderboard.API.Infrastructure.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Serilog.Context;
using Swashbuckle.AspNetCore.Annotations;

namespace Leaderboard.API.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/Leaderboard/leaderboard")]
[SwaggerTag("Leaderboard")]
public class LeaderboardController : CustomControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = @"Get the leaderboard", Description = "Get the leaderboard.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Leaderboard retrieved successfully", typeof(GetLeaderBoardResponse))]
    public async Task<GetLeaderBoardResponse> GetLeaderboard([FromQuery] GetLeaderBoardRequest request)
    {
        LogContext.PushProperty("UserId", UserId);
        if (!request.IsValid())
        {
            throw new ArgumentException("Invalid request parameters");
        }
        return await _leaderboardService.GetLeaderboardAsync(request);
    }

    [HttpGet]
    [Route("filtered")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = @"Get filtered leaderboard" ,
        Description = "Get the leaderboard with filters.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Leaderboard retrieved successfully", typeof(GetLeaderBoardResponse))]
    public async Task<GetLeaderBoardResponse> GetLeaderboardFiltered([FromQuery] LeaderboardFilterRequest request)
    {
        LogContext.PushProperty("UserId", UserId);
        if (!request.IsValid())
        {
            throw new ArgumentException("Invalid request parameters");
        }
        return await _leaderboardService.GetLeaderboardFilteredAsync(request);
    }

    [HttpGet]
    [Route("user-highest-score")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = @"Get the highest score of a user",
        Description = "Get the highest score of the current user.")]
    [SwaggerResponse(StatusCodes.Status200OK, "User highest score retrieved successfully", typeof(GetUserHighestScoreResponse))]
    public async Task<GetUserHighestScoreResponse> GetUserHighestScore()
    {
        LogContext.PushProperty("UserId", UserId);
        var request = new GetUserHighestScoreRequest
        {
            UserId = UserId
        };

        if (!request.IsValid())
        {
            throw new ArgumentException("Invalid request parameters");
        }

        return await _leaderboardService.GetUserHighestScoreAsync(request);
    }

    [HttpPost]
    [Route("score/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(SubmitScoreResponse), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Submit a score to the leaderboard",
        Description = "Submit a score to the leaderboard."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Score submitted successfully", typeof(SubmitScoreResponse))]
    public async Task<SubmitScoreResponse> SubmitScore([FromBody] SubmitScoreRequest request)
    {
        LogContext.PushProperty("UserId", UserId);

        if (!request.IsValid())
        {
            throw new ArgumentException("Invalid request parameters");
        }

        return await _leaderboardService.SubmitScore(request, UserId, DeviceType);
    }
}
