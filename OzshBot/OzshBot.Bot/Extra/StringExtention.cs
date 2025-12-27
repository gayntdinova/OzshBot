using OzshBot.Application.Services.Interfaces;
using IBLogger = OzshBot.Application.ToolsInterfaces.ILogger;

namespace OzshBot.Bot.Extra;

public static class StringExtention
{
    public static string Capitalize(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        if (str.Length == 1) return str.ToUpper();
        return str[..1].ToUpper() + str[1..].ToLower();
    }
}