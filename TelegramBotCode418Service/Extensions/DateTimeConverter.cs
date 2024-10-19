namespace TelegramBotCode418Service.Extensions;

public static class DateTimeConverter
{
    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)unixTimeStamp);
        return dateTimeOffset.DateTime;
    }

    public static double DateTimeToUnixTimeStamp(DateTime dateTime)
    {
        DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime.ToUniversalTime());
        return dateTimeOffset.ToUnixTimeSeconds();
    }

}