using System;
using System.Collections.Generic;
using System.Linq;
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
        private static TelegramBotClient _client;

        private static List<IChannel> _chatList { get; set; }

        private readonly RegHelper _regHelper;

        public TGClient()
        {
            _chatList = new List<IChannel>();
            _client = new TelegramBotClient(SecureData.GetToken());
            _regHelper = new RegHelper();
        }

        public async void StartListen()
        {
            using CancellationTokenSource cts = new CancellationTokenSource();

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = { }
            };

            _client.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);

            User me = await _client.GetMeAsync();

            Console.WriteLine($"Started receiving messages from: {me.Username}");

            cts.Cancel();
        }

        public void AddChatUser(long chatId, BotState bs)
        {
            foreach (IChannel item in _chatList)
            {
                if (item.ChatID == chatId)
                {
                    return;
                }
            }

            _chatList.Add(new ChatChannel(chatId, bs));
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

            long chatId = update.Message.Chat.Id;
            string messageText = update.Message.Text;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            AddChatUser(chatId, BotState.HANDLE_COMMAND);

            switch (_chatList.FirstOrDefault(b => b.ChatID == chatId).State)
            {
                case BotState.HANDLE_REGISTER_ANSWER:
                    bool result = _regHelper.Registration(messageText, chatId);

                    if (result == false)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Неверный формат ФИО. Попробуйте еще раз.",
                            cancellationToken: cancellationToken
                            );

                        ChangeState(_chatList, BotState.HANDLE_REGISTER_ANSWER, chatId);

                        return;
                    }

                    ChangeState(_chatList, BotState.HANDLE_REGISTER_GROUP, chatId);

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Введите ваше группу: ",
                        cancellationToken: cancellationToken
                        );

                    return;
                case BotState.HANDLE_REGISTER_GROUP:
                    bool groupResult = await _regHelper.SetGroup(messageText);

                    if (groupResult == false)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Неверный формат группы. Попробуйте еще раз.",
                            cancellationToken: cancellationToken
                            );
                        ChangeState(_chatList, BotState.HANDLE_REGISTER_GROUP, chatId);
                        return;
                    }

                    ChangeState(_chatList, BotState.HANDLE_REGISTER_COURSE, chatId);

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
                    bool courseResult = _regHelper.SetCource(messageText);

                    if (courseResult == false)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Произошла ошибка, попробуйте еще раз.",
                            cancellationToken: cancellationToken
                            );
                        ChangeState(_chatList, BotState.HANDLE_REGISTER_COURSE, chatId);
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
                           text: $"Вы {_regHelper.User.FirstName} {_regHelper.User.SecondName} {_regHelper.User.LastName} из группы {_regHelper.User.GroupByCollege.Name}?",
                           replyMarkup: replyKeyboardConfirm,
                           cancellationToken: cancellationToken
                           );

                    ChangeState(_chatList, BotState.HANDLE_REGISTER_CONFIRM, chatId);

                    return;
                case BotState.HANDLE_REGISTER_CONFIRM:
                    if (messageText == "Да")
                    {
                        await _regHelper.SaveUser();

                        await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "Вы успешно зарегестрировались!",
                           cancellationToken: cancellationToken
                           );
                        ChangeState(_chatList, BotState.HANDLE_COMMAND, chatId);
                    }

                    if (messageText == "Нет")
                    {
                        await _regHelper.SaveUser();

                        await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "Регистрация отменена",
                           cancellationToken: cancellationToken
                           );
                        ChangeState(_chatList, BotState.HANDLE_COMMAND, chatId);
                    }

                    return;
            }

            if (_chatList.FirstOrDefault(b => b.ChatID == chatId).State is BotState.HANDLE_COMMAND)
            {
                switch (messageText)
                {
                    case "/reg":
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Введите ваше ФИО (разделив одним пробелом): ",
                            cancellationToken: cancellationToken
                            );

                        ChangeState(_chatList, BotState.HANDLE_REGISTER_ANSWER, chatId);

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

        private void ChangeState(List<IChannel> bots, BotState bs, long chatId)
        {
            bots.FirstOrDefault(b => b.ChatID == chatId).SetState(bs);
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            string ErrorMessage = exception switch
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