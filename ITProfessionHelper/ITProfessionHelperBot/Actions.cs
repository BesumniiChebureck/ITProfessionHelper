using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ITProfessionHelperBot;

internal static class Actions
{
    /// <summary>
    /// Метод для вывода вакансий
    /// </summary>
    /// <param name="amountOfPrintedVacancies">Сколько вакансий уже было выведено и нужно пропустить</param>
    internal static async Task PrintVacanciesAsync(ITelegramBotClient botClient, Chat chat, int amountOfPrintedVacancies = 0)
    {
        try
        {
            var url = "https://career.habr.com/vacancies/rss?currency=RUR&sort=relevance&type=all";

            var reader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(reader);
            reader.Close();

            for (int i = 0; i < 9; i += 3)
            {
                var items = feed.Items.Skip(amountOfPrintedVacancies + i).Take(3).ToList();

                var vacanciesToCurrentMessage = new List<Vacancy>();

                Vacancy vacancyFirst = new Vacancy(items[0]);
                Vacancy vacancySecond = new Vacancy(items[1]);
                Vacancy vacancyThird = new Vacancy(items[2]);
                
                var hyperLinkKeyboard = new InlineKeyboardMarkup(
                    new List<InlineKeyboardButton[]>()
                    {
                        new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithUrl("К вакансии 1", vacancyFirst.Link),
                            InlineKeyboardButton.WithUrl("К вакансии 2", vacancySecond.Link),
                            InlineKeyboardButton.WithUrl("К вакансии 3", vacancyThird.Link)
                        }
                    });
                
                await botClient.SendTextMessageAsync
                (
                    chat.Id,
                    "1\ufe0f\u20e3 " + vacancyFirst.ToPrintableFormat() + "\n\n\n" +
                    "2\ufe0f\u20e3 " + vacancySecond.ToPrintableFormat() + "\n\n\n" +
                    "3\ufe0f\u20e3 " + vacancyThird.ToPrintableFormat(),
                    replyMarkup: hyperLinkKeyboard
                );
                
                vacanciesToCurrentMessage.Clear();
            }
            
            var inlineKeyboard = new InlineKeyboardMarkup(
                new List<InlineKeyboardButton[]>()
                {
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("Вывести следующие вакансии", $"nextVacancies{amountOfPrintedVacancies + 9}")
                    },
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("К главной", $"backToMain")
                    }
                });
                                
            await botClient.SendTextMessageAsync(
                chat.Id,
                $"Продолжить?",
                replyMarkup: inlineKeyboard);

            return;
        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync
            (
                chat.Id,
                "Извините, не удалось вывести последние 10 вакансий. Администрация получила сообщение о вашей проблеме"
            );
            
            Console.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// Метод для вывода статистика по вакансиям
    /// </summary>
    internal static async Task PrintStatisticsAsync(ITelegramBotClient botClient, Chat chat)
    {
        try
        {
            var url = "https://career.habr.com/vacancies/rss?currency=RUR&sort=relevance&type=all";

            var reader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(reader);
            reader.Close();

            //TODO: Больше статистики

            var vacancies = new List<Vacancy>();

            foreach (var item in feed.Items)
            {
                var vacancy = new Vacancy(item);

                vacancies.Add(vacancy);
            }

            var companiesStat = new StringBuilder();

            foreach (var company in vacancies.Select(p => p.Company).Distinct())
            {
                int amountVacancyWithThisCompany = vacancies.Count(vacancy => vacancy.Company == company);

                companiesStat.AppendLine($"{company}: {amountVacancyWithThisCompany}");
            }

            string statistics =
                $"\ud83d\udda5 Общее количество вакансий за сегодня: {vacancies.Count(p => p.PublishDate.Date == DateTime.Now.Date)}\n\n" +
                $"\ud83d\udcca Количество вакансий по компаниям:\n" +
                companiesStat.ToString();

            // TODO: add buttons or replce to Utils.BackToMainInlineKeyboard
            var inlineKeyboard = new InlineKeyboardMarkup(
                new List<InlineKeyboardButton[]>()
                {
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("К главной", $"backToMain")
                    }
                });

            await botClient.SendTextMessageAsync(
                chat.Id,
                statistics,
                replyMarkup: inlineKeyboard);

            return;
        }
        catch (ArgumentOutOfRangeException)
        {
            await botClient.SendTextMessageAsync
            (
                chat.Id,
                "Больше вакансий смотрите на сайте Хабр Карьера (https://career.habr.com/)",
                replyMarkup: Utils.BackToMainInlineKeyboard
            );
        }
        catch (Exception ex)
        {
            
            await botClient.SendTextMessageAsync
            (
                chat.Id,
                "Извините, не удалось рассчитать и вывести статистику. Администрация получила сообщение о вашей проблеме",
                replyMarkup: Utils.BackToMainInlineKeyboard
            );
            
            Console.WriteLine(ex.ToString());
        }
    }
}