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
using TelegramBotCode418Service.Features.HttpHandlers;
using TelegramBotCode418Service.Infrastructure;
using TelegramBotCode418Service.Models;

namespace TelegramBotCode418Service.Features.BookTicket;

[BotHandler]
public class BookTicketHandler(
    ApplicationDbContext applicationDbContext,
    HttpPostHandlers httpClientHandler)
{
    [SlashHandler("/bookticket")]
    [ReplyMenuHandler("Забронировать талон")]
    public async Task BookTicket(
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
        
        update.RegisterStepHandler(new StepTelegram(SelectCategories, new BookTicketCache()));

        var depts = GetDeptsKeyboard();
        var menu = MenuGenerator.InlineKeyboard(1, depts);
        
        var option = new OptionMessage();
        option.MenuInlineKeyboardMarkup = menu;
        
        await PRTelegramBot.Helpers.Message.Send(
            botClient,
            update, 
            "Выберите отделение:",
            option);
    }
    
    private async Task SelectCategories(ITelegramBotClient botClient, Update update)
    {
        var categories = GetCategoriesKeyboard();
        
        var menu = MenuGenerator.InlineKeyboard(1, categories);
        
        var option = new OptionMessage();
        option.MenuInlineKeyboardMarkup = menu;
        
        string msg = "Выберите категорию:";
        var handler = update.GetStepHandler<StepTelegram>();
        //handler!.GetCache<RatingCache>().Rating = (rating!.c / 100).ToString();
        handler.RegisterNextStep(SelectServices);
        await PRTelegramBot.Helpers.Message.Send(botClient, update, msg, option);
    }
    
    private async Task SelectServices(ITelegramBotClient botClient, Update update)
    {
        var services = GetServicesKeyboard();
        
        var menu = MenuGenerator.InlineKeyboard(1, services);
        
        var option = new OptionMessage();
        option.MenuInlineKeyboardMarkup = menu;
        
        string msg = "Выберите услуги:";
        var handler = update.GetStepHandler<StepTelegram>();
        //handler!.GetCache<RatingCache>().Rating = (rating!.c / 100).ToString();
        //handler.RegisterNextStep(AddRatingMessageHandle);
        await PRTelegramBot.Helpers.Message.Send(botClient, update, msg, option);
    }

    /*private async Task AddRatingMessageHandle(ITelegramBotClient botClient, Update update)
    {
        var handler = update.GetStepHandler<StepTelegram>();
        handler!.GetCache<RatingCache>().Message = update.Message?.Text ?? "";
        //handler.RegisterNextStep(SendHttpRequestWithReviewHandle);
        await PRTelegramBot.Helpers.Message.Send(botClient, update, "Спасибо за отзыв!");
        await SendHttpRequestWithReviewHandle(botClient, update);
    }*/
    
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
            ChatId = update.Message!.Chat.Id.ToString(),
            Rate = handler!.GetCache<RatingCache>().Rating,
            Message = handler.GetCache<RatingCache>().Message
        };

        await httpClientHandler.SendReviewRequestAsync(rating);
        
        handler.GetCache<RatingCache>().ClearData();
    }

    
    
    [InlineCallbackHandler<Depts>(
        Depts.Depts1,
        Depts.Depts2,
        Depts.Depts3)]
    public async Task InlineDeptsHandler(ITelegramBotClient botClient, Update update)
    {
        var command = InlineCallback<EntityTCommand<long>>
            .GetCommandByCallbackOrNull(update.CallbackQuery.Data);
        if (command is null)
        {
            string msg = $"Идентификатор который вы передали {command.Data.EntityId}";
            await PRTelegramBot.Helpers.Message.Send(botClient, update, msg);
        }
        
        await SelectCategories(botClient, update);
    }
    
    [InlineCallbackHandler<CustomHeaderCategories>(
        CustomHeaderCategories.Category1,
        CustomHeaderCategories.Category2,
        CustomHeaderCategories.Category3,
        CustomHeaderCategories.Category4)]
    public async Task InlineCategoriesHandler(ITelegramBotClient botClient, Update update)
    {
        var command = InlineCallback<EntityTCommand<long>>
            .GetCommandByCallbackOrNull(update.CallbackQuery.Data);
        if (command is null)
        {
            string msg = $"Идентификатор который вы передали {command.Data.EntityId}";
            await PRTelegramBot.Helpers.Message.Send(botClient, update, msg);
        }
        await SelectServices(botClient, update);
    }
    
    [InlineCallbackHandler<CustomHeaderServices>(
        CustomHeaderServices.Category1,
        CustomHeaderServices.Category2,
        CustomHeaderServices.Category3,
        CustomHeaderServices.Category4,
        CustomHeaderServices.Category5,
        CustomHeaderServices.Category6)]
    public async Task InlineServicesHandler(ITelegramBotClient botClient, Update update)
    {
        var command = InlineCallback<EntityTCommand<long>>
            .GetCommandByCallbackOrNull(update.CallbackQuery.Data);
        if (command is null)
        {
            string msg = $"Идентификатор который вы передали {command.Data.EntityId}";
            await PRTelegramBot.Helpers.Message.Send(botClient, update, msg);
        }
        //await AddRatingHandle(botClient, update);
    }
    
    private List<IInlineContent> GetDeptsKeyboard()
    {
        var ratingKeyboard = new List<IInlineContent>
        {
            new InlineCallback<EntityTCommand<int>>
                ("г. Донецк, ул. Артёма, 58", Depts.Depts1, new EntityTCommand<int>(900)),
            new InlineCallback<EntityTCommand<int>>
                ("г. Макеевка, ул. Центральная, 12", Depts.Depts2, new EntityTCommand<int>(1000)),
            new InlineCallback<EntityTCommand<int>>
                ("с. Славное, ул. Дмитриева, 102", Depts.Depts3, new EntityTCommand<int>(1100)),
        };
        
        return ratingKeyboard;
    }
    
    private List<IInlineContent> GetCategoriesKeyboard()
    {
        var ratingKeyboard = new List<IInlineContent>
        {
            new InlineCallback<EntityTCommand<int>>
                ("✉️ Почтовые услуги", CustomHeaderCategories.Category1, new EntityTCommand<int>(1200)),
            new InlineCallback<EntityTCommand<int>>
                ("💵 Финансовые услуги", CustomHeaderCategories.Category2, new EntityTCommand<int>(1300)),
            new InlineCallback<EntityTCommand<int>>
                ("📰 Подписка на газеты и журналы", CustomHeaderCategories.Category3, new EntityTCommand<int>(1400)),
            new InlineCallback<EntityTCommand<int>>
                ("📞 Приобрести SIM-карту", CustomHeaderCategories.Category4, new EntityTCommand<int>(1500)),
        };
        
        return ratingKeyboard;
    }
    
    private List<IInlineContent> GetServicesKeyboard()
    {
        var ratingKeyboard = new List<IInlineContent>
        {
            new InlineCallback<EntityTCommand<int>>
                ("📤 Отправить почту или посылку", CustomHeaderServices.Category1, new EntityTCommand<int>(1600)),
            new InlineCallback<EntityTCommand<int>>
                ("📥 Получить почту или посылку", CustomHeaderServices.Category2, new EntityTCommand<int>(1700)),
            new InlineCallback<EntityTCommand<int>>
                ("💸 Отправить денежный перевод", CustomHeaderServices.Category3, new EntityTCommand<int>(1800)),
            new InlineCallback<EntityTCommand<int>>
                ("💳 Получить денежный перевод", CustomHeaderServices.Category4, new EntityTCommand<int>(1900)),
            new InlineCallback<EntityTCommand<int>>
                ("🏢 Оплатить коммунальные услуги", CustomHeaderServices.Category4, new EntityTCommand<int>(2000)),
            new InlineCallback<EntityTCommand<int>>
                ("💰 Получить пенсию или пособие", CustomHeaderServices.Category4, new EntityTCommand<int>(2100)),
        };
        
        return ratingKeyboard;
    }
    
}

[InlineCommand] 
public enum Depts
{
    [Description("test1")]
    Depts1 = 900,
    [Description("test2")]
    Depts2 = 1000,
    [Description("test3")]
    Depts3 = 1100
}

[InlineCommand] 
enum CustomHeaderCategories
{
    [Description("test4")]
    Category1 = 1200,
    [Description("test5")]
    Category2 = 1300,
    [Description("test6")]
    Category3 = 1400,
    [Description("test7")]
    Category4 = 1500
}

[InlineCommand] 
enum CustomHeaderServices
{
    [Description("Отправить почту или посылку")]
    Category1 = 1600,
    [Description("Получить почту или посылку")]
    Category2 = 1700,
    [Description("Отправить денежный перевод")]
    Category3 = 1800,
    [Description("Получить денежный перевод")]
    Category4 = 1900,
    [Description("Оплатить коммунальные услуги")]
    Category5 = 2000,
    [Description("Получить пенсию или пособие")]
    Category6 = 2100
}

/*public class Response
{
    public Data d { get; set; }
    public int c { get; set; }
}

public class Data
{
    public int _1 { get; set; } // Используем _1, так как ключ начинается с цифры
    public int l { get; set; }
    public int a { get; set; }
}*/