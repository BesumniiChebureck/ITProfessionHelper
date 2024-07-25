using System.Xml;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Collections.Generic;
using System.Globalization;

namespace ITProfessionHelperBot;

static class Program
{
    // Клиент для работы с Telegram Bot API
    private static ITelegramBotClient _botClient;

    // Объект с настройками работы бота
    private static ReceiverOptions _receiverOptions;


    static async Task Main()
    {
        _botClient = new TelegramBotClient("INSER YOUT TOKEN FROM BOTFATHER");
        _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message,
                UpdateType.CallbackQuery
            },
            ThrowPendingUpdates = true,
        };

        using var cts = new CancellationTokenSource();

        // Запуск бота
        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

        // Объект с информацией о боте
        var me = await _botClient.GetMeAsync();

        // TODO: add start and end info in Log system

        Console.WriteLine($"{me.FirstName} запущен!");

        // Обработка завершения работы бота
        var adminMessage = Console.ReadLine();

        while (!adminMessage.ToLower().Contains("stop"))
        {
            adminMessage = Console.ReadLine();
        }
    }

    /// <summary>
    /// Обработчик приходящих Update
    /// </summary>
    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        // блок try-catch, чтобы бот в случае чего не упал
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    // Сообщение пользователя
                    var message = update.Message;

                    // Объект пользователя, от которого пришло сообщение
                    var user = message.From;

                    // TODO: (may be) add send message from user to Log system
                    Console.WriteLine($"{user.Username} ({user.Id}) написал сообщение: {message.Text}");

                    // Объект чата с пользователем
                    var chat = message.Chat;

                    // Проверка на тип Message
                    switch (message.Type)
                    {
                        //Если сообщение имеет текстовый тип
                        case MessageType.Text:
                        {
                            // Обработчики команд (may be switch ?)

                            // Загрузка Reply клавиатуры Start

                            if (message.Text?.ToLower() == "/start")
                            {
                                await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    $"Добро пожаловать, {user.FirstName}!\n" +
                                    $"Я бот-помощник по поиску IT-вакансий и в качестве источника использую Хабр Карьера (https://career.habr.com/)",
                                    replyMarkup: Utils.MainReplyKeyboard);

                                return;
                            }

                            // Загрузка Reply клавиатуры End

                            if (message.Text?.ToLower() == "вывести вакансии")
                            {
                                await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    $"Выполняю..",
                                    replyMarkup: new ReplyKeyboardRemove(),
                                    replyToMessageId: message.MessageId);

                                await Actions.PrintVacanciesAsync(botClient, chat);

                                return;
                            }
                            
                            if (message.Text?.ToLower() == "вывести статистику")
                            {
                                await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    $"Выполняю..",
                                    replyMarkup: new ReplyKeyboardRemove(),
                                    replyToMessageId: message.MessageId);

                                await Actions.PrintStatisticsAsync(botClient, chat);

                                return;
                            }
                            
                            if (message.Text?.ToLower() == "эхо ответ")
                            {
                                //
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    message.Text,
                                    replyToMessageId: message.MessageId
                                );

                                return;
                            }

                            //Сообщение, если введенно что-то непонятное
                            await botClient.SendTextMessageAsync(
                                chat.Id,
                                $"Извините, не могу понять ваш запрос. Введите команду /start",
                                replyMarkup: new ReplyKeyboardRemove());

                            return;
                        }
                    }

                    return;
                }

                case UpdateType.CallbackQuery:
                {
                    // Объект нажатой кнопки с информацией о ней
                    var callbackQuery = update.CallbackQuery;

                    // Пользователь, который нажал кнопку
                    var user = callbackQuery.From;

                    Console.WriteLine($"{user.FirstName} ({user.Id}) нажал кнопку: {callbackQuery.Data}");

                    // Чат, в котором нажали на кнопку
                    var chat = callbackQuery.Message.Chat;

                    if (callbackQuery.Data.Contains("nextVacancies"))
                    {
                        // Отправить телеграмму запрос, что была нажата кнопка
                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                        int amountOfPrintedVacancies = Convert.ToInt32(callbackQuery.Data[13..]);

                        await Actions.PrintVacanciesAsync(botClient, chat, amountOfPrintedVacancies);

                        return;
                    }

                    if (callbackQuery.Data == "backToMain")
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                        
                        await botClient.SendTextMessageAsync(
                            chat.Id,
                            $"Вы на главной, {user.FirstName}\n\n" +
                            $"Выберите, что мне необходимо сделать",
                            replyMarkup: Utils.MainReplyKeyboard);

                        return;
                    }

                    return;
                }
            }
        }
        catch (Exception ex)
        {
            // TODO: add user input Exception to Log system
            Console.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// Обработчик ошибок, связанных с Telegram Bot API
    /// </summary>
    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error,
        CancellationToken cancellationToken)
    {
        var errorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n" +
                   $"\tErrorCode: [{apiRequestException.ErrorCode}]\n" +
                   $"\tErrorMessage: [{apiRequestException.Message}]",
            _ => error.ToString(),
        };

        // TODO: add Error Exception to Log system
        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}