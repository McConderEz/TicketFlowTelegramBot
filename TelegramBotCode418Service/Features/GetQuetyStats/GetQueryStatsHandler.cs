using PRTelegramBot.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotCode418Service.Features.GetQuetyStats;

[BotHandler]
public class GetQueryStatsHandler
{

    [SlashHandler("/stats")]
    [ReplyMenuHandler("Состояние очереди")]
    public async Task GetQueryStatsHandle(ITelegramBotClient botClient, Update update)
    {
        await PRTelegramBot.Helpers.Message.Send(botClient, update,
            "" +
            "Электронная очередь\n\nКлиент  Статус     Окно\nH59         Ожидание  -\nH61         Ожидание  -\nB3           Ожидание  -\nF5            Ожидание  -\n\nВсего людей в очереди: 4");
    }
}