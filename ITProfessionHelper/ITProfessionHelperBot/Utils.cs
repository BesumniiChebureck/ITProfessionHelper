using System.Globalization;
using Telegram.Bot.Types.ReplyMarkups;

namespace ITProfessionHelperBot;

internal static class Utils
{
    /// <summary>
    /// Расширение DateTime, позволяющий получить из dateTime строку в формате: [день месяца] [название месяца на русском языке] [время]
    /// </summary>
    internal static string PrintableFormat(this DateTime dateTime)
        => $"{dateTime.Day} {dateTime.ToString("MMMM", new CultureInfo("ru-RU"))[..^1]}. {dateTime.ToShortTimeString()}";
    
    /// <summary>
    /// Клавиатура, содержащая только 1 кнопку - вернуться к главной
    /// </summary>
    internal static readonly InlineKeyboardMarkup BackToMainInlineKeyboard =
        new InlineKeyboardMarkup
        (
            InlineKeyboardButton.WithCallbackData("К главной", $"backToMain")
        );
    
    /// <summary>
    /// Клавиатура главной
    /// </summary>
    internal static readonly ReplyKeyboardMarkup MainReplyKeyboard = new ReplyKeyboardMarkup(
        new List<KeyboardButton[]>()
        {
            new KeyboardButton[]
            {
                new KeyboardButton("Вывести вакансии")
            },
            new KeyboardButton[]
            {
                new KeyboardButton("Вывести статистику")
            }
        })
    {
        // Автоматическое изменение размера клавиатуры
        ResizeKeyboard = true,
    };
}