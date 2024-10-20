using PRTelegramBot.Interfaces;

namespace TelegramBotCode418Service.Cache;

public class RatingCache: ITelegramCache
{
    public string Rating { get; set; }
    public string Message { get; set; }
    
    public bool ClearData()
    {
        Rating = string.Empty;
        Message = string.Empty;
        return true;
    }
}