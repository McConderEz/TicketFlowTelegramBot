using PRTelegramBot.Interfaces;

namespace TelegramBotCode418Service.Cache;

public class BookTicketCache: ITelegramCache
{
    public string Dept { get; set; }
    public string Category { get; set; }
    public string Service { get; set; }
    public DateTime Time { get; set; }
    
    public bool ClearData()
    {
        Dept = string.Empty;
        Category = string.Empty;
        Time = DateTime.Now;
        Service = string.Empty;
        return true;
    }
}