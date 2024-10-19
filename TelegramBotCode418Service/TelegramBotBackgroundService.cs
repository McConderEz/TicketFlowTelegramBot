using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRTelegramBot.Attributes;
using PRTelegramBot.Core;
using PRTelegramBot.Models;
using PRTelegramBot.Models.Enums;
using PRTelegramBot.Models.EventsArgs;
using PRTelegramBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotCode418Service.Config;
using TelegramBotCode418Service.Extensions;
using TelegramBotCode418Service.Features.Authentication;
using TelegramBotCode418Service.Infrastructure;
using TelegramBotCode418Service.Shared;
using User = TelegramBotCode418Service.Infrastructure.Entities.User;


namespace TelegramBotCode418Service;

public class TelegramBotBackgroundService : BackgroundService
{
    private readonly ILogger<TelegramBotBackgroundService> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly Options.TelegramOptions _telegramOptions;

    public static OptionMessage OptionMessage { get; private set; }

    public TelegramBotBackgroundService(
        ILogger<TelegramBotBackgroundService> logger,
        IOptions<Options.TelegramOptions> telegramBotOptions,
        IServiceProvider serviceProvider, 
        ITelegramBotClient botClient,
        ApplicationDbContext applicationDbContext)
    {
        _logger = logger;
        _telegramOptions = telegramBotOptions.Value;
        _serviceProvider = serviceProvider;
        _botClient = botClient;
        _applicationDbContext = applicationDbContext;
        OptionMessage = SeedReplyKeyboardMarkup();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = []
        };
        
        var telegram = new PRBotBuilder(
                _telegramOptions.Token 
                ?? throw new ApplicationException("Cannot get token"))
            .SetBotId(0)
            .SetClearUpdatesOnStart(true)
            .SetServiceProvider(_serviceProvider)
            .Build();

        telegram.Events.UpdateEvents.OnPreUpdate += On_PreUpdate;
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await telegram.Start();
        }
    }
    
    private async Task<UpdateResult> On_PreUpdate(BotEventArgs args)
    {
        Console.WriteLine();
        
        return UpdateResult.Continue;
    }
    
    
    private Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(errorMessage);
        return Task.CompletedTask;
    }

    
    private async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken = default)
    {
        if (update.Message is { } message)
        {
            if (message.Text is not { } messageText)
                return;
            var splitMessage = messageText.Split(' ');
            if (splitMessage.Length != 2)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, 
                    text: "Вы не аутентифицированы", 
                    cancellationToken: cancellationToken);
                return;
            }

            await Authentication(message.Chat.Id, splitMessage, cancellationToken);
        }
    }
    
    private async Task<bool> IsAuthenticated(long chatId, CancellationToken cancellationToken)
    {
        var user = await _applicationDbContext.Users
            .FirstOrDefaultAsync(u => u.ChatId == chatId.ToString(), cancellationToken);
        return user != null;
    }
    
    private async Task Authentication(ChatId chatId,string[] data,CancellationToken cancellationToken)
    {
        if (!double.TryParse(data[1], out double unixTime))
            return;

        var createdAt = DateTimeConverter.UnixTimeStampToDateTime(unixTime);
        
        if(createdAt.AddMinutes(1) < DateTime.Now)
            return;
        
        var user = new User { ChatId = chatId.ToString() };
        await _applicationDbContext.Users.AddAsync(user,cancellationToken);
        await _applicationDbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Пользователь создан с chatId {chatId}", chatId.ToString());
        
        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Добро пожаловать в телеграм-бот почтовой очереди",
            replyMarkup: OptionMessage.MenuReplyKeyboardMarkup,
            cancellationToken: cancellationToken);

    }
    
    private OptionMessage SeedReplyKeyboardMarkup()
    {
        var json =  System.IO.File.ReadAllText(Constants.PathMenuJson);

        var seedData = JsonSerializer.Deserialize<MenuFeaturesConfig>(json)
                       ?? throw new ApplicationException("could not deserialize menu keyboards");

        var keyBoards = new List<KeyboardButton>();
    
        foreach (var menuFeature in seedData.Menu.Keyboards)
        {
            keyBoards.Add(menuFeature);
        }
        
        var menu = MenuGenerator.ReplyKeyboard(1, keyBoards);
        var option = new OptionMessage();
        option.MenuReplyKeyboardMarkup = menu;
        
        return option;
    }

}