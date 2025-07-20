using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Leaderboard.API.Models;
using Leaderboard.API.Infrastructure.Services;
using Leaderboard.API.Infrastructure.Models.Dtos;
using Serilog.Context;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace Leaderboard.API.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/Leaderboard/user")]
[SwaggerTag("User")]
public class UserController : CustomControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public UserController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    [HttpGet]
    [Route("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get user stats", Description = "Get user stats.")]
    [SwaggerResponse(StatusCodes.Status200OK, "User stats retrieved successfully", typeof(GetUserStatsResponse))]
    public async Task<GetUserStatsResponse> GetUserStats()
    {
        LogContext.PushProperty("UserId", UserId);
        return await _leaderboardService.GetUserStatsAsync(UserId);
    }
}
