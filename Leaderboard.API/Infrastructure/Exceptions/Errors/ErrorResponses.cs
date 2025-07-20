using System.Net;
using Leaderboard.API.Infrastructure.Errors;

namespace Leaderboard.API.Infrastructure.Errors;

public static class ErrorResponses
{
    public static ErrorResponse CompanyNotFound = new ErrorResponse(
        HttpStatusCode.NotFound,
        "The company with the given ID does not exist",
        "الشركة بالمعرف المحدد غير موجودة",
        "NOT_FOUND");

    public static ErrorResponse InvalidRequest = new ErrorResponse(
        HttpStatusCode.BadRequest,
        "The request is invalid",
        "الطلب غير صالح",
        "INVALID_REQUEST");
    public static ErrorResponse UserNotFound = new ErrorResponse(
        HttpStatusCode.NotFound,
        "The user with the given ID does not exist",
        "المستخدم بالمعرف المحدد غير موجود",
        "NOT_FOUND");
    public static ErrorResponse UserAlreadyExists = new ErrorResponse(
        HttpStatusCode.Conflict,
        "The user with the given ID already exists",
        "المستخدم بالمعرف المحدد موجود بالفعل",
        "USER_ALREADY_EXISTS");
    public static ErrorResponse InvalidScore = new ErrorResponse(
        HttpStatusCode.BadRequest,
        "The score is invalid",
        "النتيجة غير صالحة",
        "INVALID_SCORE");
    public static ErrorResponse InvalidLevel = new ErrorResponse(
        HttpStatusCode.BadRequest,
        "The level is invalid",
        "المستوى غير صالح",
        "INVALID_LEVEL");
}
