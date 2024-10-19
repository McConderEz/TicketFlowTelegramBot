using PRTelegramBot.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TelegramBotCode418Service.Extensions;
using TelegramBotCode418Service.Infrastructure;
using User = TelegramBotCode418Service.Infrastructure.Entities.User;

namespace TelegramBotCode418Service.Features.Authentication;

public class UpdateHandler(
    ApplicationDbContext applicationDbContext,
    ITelegramBotClient bot,
    ILogger<UpdateHandler> logger) : IUpdateHandler
{
    private static readonly InputPollOption[] PollOptions = ["Hello", "World!"];

    public async Task HandleErrorAsync(
        ITelegramBotClient botClient, 
        Exception exception, 
        HandleErrorSource source, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation("HandleError: {Exception}", exception);
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await (update switch
        {
            { Message: { } message } => Authentication(
                message.Chat.Id,
                message.Text!.Split(' '), 
                cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(update), update, null)
        });
    }
    
    private async Task Authentication(ChatId chatId,string[] data,CancellationToken cancellationToken)
    {
        if (!double.TryParse(data[1], out double unixTime))
            return;

        var createdAt = DateTimeConverter.UnixTimeStampToDateTime(unixTime);
        
        if(createdAt.AddMinutes(1) < DateTime.Now)
            return;
        
        var user = new User { ChatId = chatId.ToString() };
        await applicationDbContext.Users.AddAsync(user,cancellationToken);
        await applicationDbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Пользователь создан с chatId {chatId}", chatId.ToString());
        
        await bot.SendTextMessageAsync(
            chatId: chatId,
            text: "Добро пожаловать в телеграм-бот почтовой очереди",
            cancellationToken: cancellationToken);

    }
    
}