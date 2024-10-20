using Microsoft.EntityFrameworkCore;
using PRTelegramBot.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotCode418Service.Abstraction;
using TelegramBotCode418Service.Infrastructure;

namespace TelegramBotCode418Service.Features.GetInformation;

[BotHandler]
public class GetInformationHandler(
    ApplicationDbContext applicationDbContext,
    ILogger<GetInformationHandler> logger) : IHandler
{
    
    [SlashHandler("/help")]
    [ReplyMenuHandler("Справочная информация")]
    public async Task Handle(
        ITelegramBotClient botClient, 
        Update update)
    {
        var isAuthenticated = await applicationDbContext.IsAuthenticated(update.Message!.Chat.Id);
        if (!isAuthenticated)
        {
            await PRTelegramBot.Helpers.Message.Send(
                botClient, update, "Вы не аутентифицированы!");
            return;
        }

        var message = "Бот позволяет бронировать талоны на услуги, осуществляемые отделениями Почты Донбасса.\n\n" +
                      "🎫  Команда /new (Взять талон) позволяет забронировать талон на выбранную услугу" +
                      " на определённую дату и время.\n📊  Команда" +
                      " /stats позволяет получить информацию о текущем состоянии очереди" +
                      " и прогнозируемой загруженности в выбранные даты.\n ⭐️ " +
                      "Команда /review (Отзыв) позволяет оставить отзыв на работу сервиса." +
                      "\n❓ Команда /help (Помощь) позволяет вывести данное сообщение.\n\n" +
                      "💬 Если у вас остались вопросы, то команда " +
                      "/support (Поддержка) позволяет задать вопрос оператору техподдержки.";
        
        await PRTelegramBot.Helpers.Message.Send(
            botClient, update, message, TelegramBotBackgroundService.OptionMessage);
        logger.LogInformation("Справочная информация отправлена пользователю с chatId {chatId}",
            update.Message!.Chat.Id.ToString());
    }
}