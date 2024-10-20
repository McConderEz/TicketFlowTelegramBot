using System.ComponentModel;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PRTelegramBot.Attributes;
using PRTelegramBot.Extensions;
using PRTelegramBot.Interfaces;
using PRTelegramBot.Models;
using PRTelegramBot.Models.CallbackCommands;
using PRTelegramBot.Models.InlineButtons;
using PRTelegramBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotCode418Service.Cache;
using TelegramBotCode418Service.Infrastructure;
using TelegramBotCode418Service.Models;

namespace TelegramBotCode418Service.Features.AddRating;

[BotHandler]
public class AddRatingHandler(ApplicationDbContext applicationDbContext)
{
    [SlashHandler("/review")]
    [ReplyMenuHandler("Оставить отзыв")]
    public async Task SendFeedback(
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
        
        update.RegisterStepHandler(new StepTelegram(AddRatingHandle, new RatingCache()));

        var rating = GetRatingKeyboard();
        var menu = MenuGenerator.InlineKeyboard(5, rating);
        
        var option = new OptionMessage();
        option.MenuInlineKeyboardMarkup = menu;
        
        await PRTelegramBot.Helpers.Message.Send(
            botClient,
            update, 
            "Поставьте оценку:",
            option);
    }
    
    private async Task AddRatingHandle(ITelegramBotClient botClient, Update update)
    {
        var rating = JsonSerializer.Deserialize<Response>(update.CallbackQuery!.Data);
        string msg = "Введите комментарий к отзыву:";
        var handler = update.GetStepHandler<StepTelegram>();
        handler!.GetCache<RatingCache>().Rating = (rating!.c / 100).ToString();
        handler.RegisterNextStep(AddRatingMessageHandle);
        await PRTelegramBot.Helpers.Message.Send(botClient, update, msg);
    }

    private async Task AddRatingMessageHandle(ITelegramBotClient botClient, Update update)
    {
        var handler = update.GetStepHandler<StepTelegram>();
        handler!.GetCache<RatingCache>().Message = update.Message?.Text ?? "";
        //handler.RegisterNextStep(SendHttpRequestWithReviewHandle);
        await PRTelegramBot.Helpers.Message.Send(botClient, update, "Спасибо за отзыв!");
        await SendHttpRequestWithReviewHandle(botClient, update);
    }
    
    private async Task SendHttpRequestWithReviewHandle(ITelegramBotClient botClient, Update update)
    {
        var handler = update.GetStepHandler<StepTelegram>();
        update.ClearStepUserHandler();

        var user = await applicationDbContext.Users
            .FirstOrDefaultAsync(u => u.ChatId == update.Message!.Chat.Id.ToString());
        
        if (user is null)
            return;

        var rating = new Rating
        {
            Rate = handler.GetCache<RatingCache>().Rating,
            Message = handler.GetCache<RatingCache>().Message
        };
        
        //TODO: Послать http
        
        handler.GetCache<RatingCache>().ClearData();
    }

    
    
    [InlineCallbackHandler<CustomTHeader>(
        CustomTHeader.VeryBad,
        CustomTHeader.Bad,
        CustomTHeader.Middle,
        CustomTHeader.Good,
        CustomTHeader.Excellent)]
    public async Task InlineHandler(ITelegramBotClient botClient, Update update)
    {
        var command = InlineCallback<EntityTCommand<long>>
            .GetCommandByCallbackOrNull(update.CallbackQuery.Data);
        if (command is null)
        {
            string msg = $"Идентификатор который вы передали {command.Data.EntityId}";
            await PRTelegramBot.Helpers.Message.Send(botClient, update, msg);
        }
        await AddRatingHandle(botClient, update);
    }
    
    private List<IInlineContent> GetRatingKeyboard()
    {
        var ratingKeyboard = new List<IInlineContent>
        {
            new InlineCallback<EntityTCommand<int>>
                ("😡", CustomTHeader.VeryBad, new EntityTCommand<int>(1)),
            new InlineCallback<EntityTCommand<int>>
                ("😞", CustomTHeader.Bad, new EntityTCommand<int>(2)),
            new InlineCallback<EntityTCommand<int>>
                ("😐", CustomTHeader.Middle, new EntityTCommand<int>(3)),
            new InlineCallback<EntityTCommand<int>>
                ("😊", CustomTHeader.Good, new EntityTCommand<int>(4)),
            new InlineCallback<EntityTCommand<int>>
                ("🤩", CustomTHeader.Excellent, new EntityTCommand<int>(5))
        };
        
        return ratingKeyboard;
    }
    
    [InlineCommand] 
    enum CustomTHeader
    {
        [Description("Очень плохо")]
        VeryBad = 100,
        [Description("Плохо")]
        Bad = 200,
        [Description("Нормально")]
        Middle = 300,
        [Description("Хорошо")]
        Good = 400,
        [Description("Великолепно")]
        Excellent = 500,
    }

}

public class Response
{
    public Data d { get; set; }
    public int c { get; set; }
}

public class Data
{
    public int _1 { get; set; } // Используем _1, так как ключ начинается с цифры
    public int l { get; set; }
    public int a { get; set; }
}