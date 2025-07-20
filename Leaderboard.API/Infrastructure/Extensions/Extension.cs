using System.Text.RegularExpressions;
using Leaderboard.API.Models.Enums;

namespace Leaderboard.API.Infrastructure.Extensions;

public static class Extension
{
    public static LanguageEnum FromStringLanguage(this string language)
    {

        if (language == "ar")
            return LanguageEnum.Arabic;

        else
            return LanguageEnum.English;

    }

    public static bool IsValidObjectId(this string id)
    {
        // Regular expression to check if the string is a valid 24-character hexadecimal string
        var objectIdPattern = new Regex("^[a-fA-F0-9]{24}$");
        return objectIdPattern.IsMatch(id);
    }

}
