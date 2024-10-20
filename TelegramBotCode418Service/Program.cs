using Microsoft.Extensions.Options;
using PRTelegramBot.Core;
using PRTelegramBot.Extensions;
using Telegram.Bot;
using TelegramBotCode418Service;
using TelegramBotCode418Service.Features.HttpHandlers;
using TelegramBotCode418Service.Infrastructure;
using TelegramBotCode418Service.Options;


const string EXIT_COMMAND = "exit";



var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<TelegramBotBackgroundService>();
builder.Services.AddSingleton<ApplicationDbContext>();

builder.Services.AddBotHandlers();

builder.Services.AddTransient<ITelegramBotClient, TelegramBotClient>(provider =>
{
    var token = provider.GetRequiredService<IOptions<TelegramOptions>>().Value.Token;

    return new(token);
});
builder.Services.AddSingleton<HttpPostHandlers>();

builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection(TelegramOptions.Telegram));

var host = builder.Build();

host.Run();

while (true)
{
    var result = Console.ReadLine();
    if (result.ToLower() == EXIT_COMMAND)
        Environment.Exit(0);
}