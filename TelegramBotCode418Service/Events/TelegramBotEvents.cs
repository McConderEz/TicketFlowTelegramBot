﻿using Microsoft.EntityFrameworkCore;
using PRTelegramBot.Models;
using PRTelegramBot.Models.EventsArgs;
using TelegramBotCode418Service.Extensions;
using TelegramBotCode418Service.Infrastructure;
using TelegramBotCode418Service.Infrastructure.Entities;

namespace TelegramBotCode418Service.Events;

public class TelegramBotEvents(ApplicationDbContext applicationDbContext)
{
    public async Task OnUserStartWithArgs(StartEventArgs args)
    {
        var chatId = args.Update.Message!.Chat.Id.ToString();
    
        if (!double.TryParse(args.Data, out double unixTime))
            return;

        var createdAt = DateTimeConverter.UnixTimeStampToDateTime(unixTime);
    
        if (createdAt.AddMinutes(90) < DateTime.Now)
            return;
        
        var userExists = await applicationDbContext.Users.AnyAsync(u => u.ChatId == chatId);
    
        if (!userExists)
        {
            var user = new User { ChatId = chatId };
        
            await applicationDbContext.Users.AddAsync(user);
            await applicationDbContext.SaveChangesAsync();
        
            await PRTelegramBot.Helpers.Message.Send(
                args.BotClient,
                args.Update,
                "Добро пожаловать в телеграм-бот почтовой очереди",
                TelegramBotBackgroundService.OptionMessage);
        }
        else
        {
            await PRTelegramBot.Helpers.Message.Send(
                args.BotClient,
                args.Update,
                "Вы уже зарегистрированы в телеграм-боте.",
                TelegramBotBackgroundService.OptionMessage);
        }
    }
}
