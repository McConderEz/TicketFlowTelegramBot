
using Microsoft.EntityFrameworkCore;
using PRTelegramBot.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotCode418Service.Abstraction;
using TelegramBotCode418Service.Infrastructure;

namespace TelegramBotCode418Service.Features.GetSupport;

[BotHandler]
public class GetSupportHandler(ApplicationDbContext applicationDbContext,ILogger<GetSupportHandler> logger) : IHandler
{
    [SlashHandler("/support")]
    [ReplyMenuHandler("Получить контакты поддержки")]
    public async Task Handle(
        ITelegramBotClient botClient, 
        Update update)
    {
        var isAuthenticated = await applicationDbContext.IsAuthenticated(update.Message!.Chat.Id);
        if (!isAuthenticated)
        {
            await PRTelegramBot.Helpers.Message.Send(
                botClient, update, "Новенький, пошёл нахуй с чата");
            return;
        }
        
        var supportsPhoneNumbers = new List<Contact>
        {
            new() { FirstName = "Сергей", PhoneNumber = "+79494271795"},
            new() { FirstName = "Владислав", PhoneNumber = "+79494814335"},
            new() { FirstName = "Дмитрий", PhoneNumber = "+79493563776"}
        };

        foreach (var contact in supportsPhoneNumbers)
            await botClient.SendContactAsync(
                update.Message!.Chat.Id,
                contact.PhoneNumber,
                contact.FirstName,
                replyMarkup: TelegramBotBackgroundService.OptionMessage.MenuReplyKeyboardMarkup);
        
        /*wait PRTelegramBot.(
            botClient, update, "Контакты", TelegramBotBackgroundService.OptionMessage);*/
        logger.LogInformation("Контакты отправлены пользователю с chatId {chatId}",
            update.Message!.Chat.Id.ToString());
    }
}