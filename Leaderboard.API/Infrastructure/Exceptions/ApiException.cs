using Leaderboard.API.Infrastructure.Errors;

namespace Leaderboard.API.Infrastructure.Exceptions;

public class ApiException : Exception
{

    public ErrorResponse Error { get; }

    public ApiException(ErrorResponse error) : base(error.ErrorDescription)
    {
        Error = error;
    }
}
