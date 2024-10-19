using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotCode418Service.Abstraction;

public interface IHandler
{
    Task Handle(
        ITelegramBotClient botClient, 
        Update update);
}