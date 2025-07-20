using System.Net;
using Microsoft.AspNetCore.Mvc;
using Leaderboard.API.Models.Enums;

namespace Leaderboard.API.Infrastructure.Errors;

public class ErrorResponse
{
    public string MessageAr { get; set; }
    public string MessageEn { get; set; }
    public string DetailAr { get; set; }
    public string DetailEn { get; set; }
    public string Type { get; set; }
    public HttpStatusCode HttpStatusCode { get; set; }
    public string ErrorDescription
    {
        get
        {
            return $"Status {HttpStatusCode} and message {MessageEn}";
        }
    }

    public ErrorResponse(HttpStatusCode httpStatusCode, string messageEn, string messageAr, string type = "about:blank")
    {
        MessageAr = messageAr;
        MessageEn = messageEn;
        Type = type;
        HttpStatusCode = httpStatusCode;
    }

    public ErrorResponse(HttpStatusCode httpStatusCode, string messageEn, string messageAr, string detailEn, string detailAr, string type = "about:blank")
    {
        MessageAr = messageAr;
        MessageEn = messageEn;
        DetailAr = detailAr;
        DetailEn = detailEn;
        Type = type;
        HttpStatusCode = httpStatusCode;
    }

    public ProblemDetails GetProblemDetails(LanguageEnum language, IServiceProvider serviceProvider)
    {
        ProblemDetails problemDetails = new ProblemDetails
        {
            Title = language == LanguageEnum.English ? MessageEn : MessageAr,
            Detail = language == LanguageEnum.English ? DetailEn : DetailAr,
            Status = (int)HttpStatusCode,
            Type = Type,
        };

        return problemDetails;
    }

}
