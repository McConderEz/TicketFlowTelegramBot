using PRTelegramBot.Attributes;
using PRTelegramBot.Models;
using PRTelegramBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotCode418Service.Abstraction;

namespace TelegramBotCode418Service.Features.GetSupport;

[BotHandler]
public class GetInformationHandler(ILogger<GetInformationHandler> logger) : IHandler
{
    [ReplyMenuHandler("/help")]
    public async Task Handle(
        ITelegramBotClient botClient, 
        Update update)
    {
        await PRTelegramBot.Helpers.Message.Send(
            botClient, update, "Справочная информация", TelegramBotBackgroundService.OptionMessage);
        logger.LogInformation("Контакты отправлены пользователю с chatId {chatId}", update.Message!.Chat.Id.ToString());
    }
}