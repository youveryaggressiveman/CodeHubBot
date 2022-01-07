using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelegBOT.Core.StateMachine;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegBOT.Core
{
    public class TGClient
    {
        private static TelegramBotClient client;

        public List<IBot> BotChatList { get; set; }
        RegHelper regHelper = new RegHelper();

        public TGClient()
        {
            BotChatList = new List<IBot>();
            client = new TelegramBotClient(SecureData.GetToken());
        }

        public async void StartListen()
        {
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = { }
            };

            client.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);

            var me = await client.GetMeAsync();

            Console.WriteLine($"Начал слушать {me.Username}");

            Console.ReadLine();

            cts.Cancel();
        }

        public void AddChatUser(long chatID, BotState bs)
        {
            foreach (var item in BotChatList)
            {
                if (item.ChatID == chatID)
                {
                    return;
                }
            }

            BotChatList.Add(new Bot(chatID, bs));
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                return;
            }

            if (update.Message!.Type != Telegram.Bot.Types.Enums.MessageType.Text)
            {
                return;
            }

            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            AddChatUser(chatId, BotState.HANDLE_COMMAND);


            switch (BotChatList.FirstOrDefault(b => b.ChatID == chatId).State)
            {
                case BotState.HANDLE_REGISTER_ANSWER:
                    var result = regHelper.Registration(messageText);
                    if (result == false)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Неверный формат ФИО. Попробуйте еще раз.",
                            cancellationToken: cancellationToken
                            );
                        ChangeState(BotChatList, BotState.HANDLE_REGISTER_ANSWER, chatId);
                        return;
                    }
                    ChangeState(BotChatList, BotState.HANDLE_REGISTER_GROUP, chatId);
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Введите ваше группу: ",
                        cancellationToken: cancellationToken
                        );
                    return;
                case BotState.HANDLE_REGISTER_GROUP:
                    var groupResult = await regHelper.SetGroup(messageText);
                    if (groupResult == false)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Неверный формат группы. Попробуйте еще раз.",
                            cancellationToken: cancellationToken
                            );
                        ChangeState(BotChatList, BotState.HANDLE_REGISTER_GROUP, chatId);
                        return;
                    }
                    ChangeState(BotChatList, BotState.HANDLE_REGISTER_COURSE, chatId);
                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] {
                        new KeyboardButton[]{
                            "Мобильная разработка"
                        },
                        new KeyboardButton[]{
                            "Разработка desktop-приложений"
                        },
                         new KeyboardButton[]{
                            "Разработка комплексных ИС"
                        },
                    });
                    await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "Выберите направление.",
                           replyMarkup: replyKeyboardMarkup,
                           cancellationToken: cancellationToken
                           );
                    break;
                case BotState.HANDLE_REGISTER_COURSE:
                    var courseResult = regHelper.SetCource(messageText);
                    if (courseResult == false)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Произошла ошибка, попробуйте еще раз.",
                            cancellationToken: cancellationToken
                            );
                        ChangeState(BotChatList, BotState.HANDLE_REGISTER_COURSE, chatId);
                        return;
                    }
                    ReplyKeyboardMarkup replyKeyboardConfirm = new(new[] {
                        new KeyboardButton[]{
                            "Да",
                            "Нет"
                        },
                    });
                    await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: $"Вы {regHelper.User.FirstName} {regHelper.User.SecondName} {regHelper.User.LastName} из группы {regHelper.User.GroupByCollege.Name}?",
                           replyMarkup: replyKeyboardConfirm,

                           cancellationToken: cancellationToken
                           );
                    ChangeState(BotChatList, BotState.HANDLE_REGISTER_CONFIRM, chatId);
                    return;
                case BotState.HANDLE_REGISTER_CONFIRM:
                    if (messageText == "Да")
                    {
                        await regHelper.SaveUser();
                        await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "Вы успешно зарегестрировались!",
                           cancellationToken: cancellationToken
                           );
                        ChangeState(BotChatList, BotState.HANDLE_COMMAND, chatId);
                    }
                    if (messageText == "Нет")
                    {
                        await regHelper.SaveUser();
                        await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "Регистрация отменена",
                           cancellationToken: cancellationToken
                           );
                        ChangeState(BotChatList, BotState.HANDLE_COMMAND, chatId);
                    }
                    return;
                default:
                    break;
            }

            if (BotChatList.FirstOrDefault(b => b.ChatID == chatId).State is BotState.HANDLE_COMMAND)
            {
                switch (messageText)
                {
                    case "/reg":
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Введите ваше ФИО (разделив одним пробелом): ",
                            cancellationToken: cancellationToken
                            );
                        ChangeState(BotChatList, BotState.HANDLE_REGISTER_ANSWER, chatId);
                        break;

                    default:
                        await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "Неверная комманда. ",
                           cancellationToken: cancellationToken
                           );
                        break;
                }
            }


        }

        private void ChangeState(List<IBot> bots, BotState bs, long chatId)
        {
            bots.FirstOrDefault(b => b.ChatID == chatId).SetState(bs);
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}

