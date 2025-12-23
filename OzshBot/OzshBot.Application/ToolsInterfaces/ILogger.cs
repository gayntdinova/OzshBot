namespace OzshBot.Application.ToolsInterfaces;

public interface ILogger
{
    Task Log(long tgId, bool success);
}