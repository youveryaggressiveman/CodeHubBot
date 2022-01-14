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

        public List<IChannel> BotChatList { get; set; }
        RegHelper regHelper = new RegHelper();
        UserHelper userHelper = new UserHelper();
        BroadcastHelper broadcastHelper = new BroadcastHelper();

        public TGClient()
        {
            BotChatList = new List<IChannel>();
            client = new TelegramBotClient(SecureData.GetToken());
        }

        public async Task StartListen()
        {
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = { }
            };

            client.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);

            var me = await client.GetMeAsync();

            Console.WriteLine($"Начал слушать {me.Username}");

            await LoggerSinglton.GetFileManager().WriteToFile(LoggerSinglton.FileInfo, $"Начал слушать {me.Username}");

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

            BotChatList.Add(new ChatChannel(chatID, bs));
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

            await LoggerSinglton.GetFileManager().WriteToFile(LoggerSinglton.FileInfo, $"Received a '{messageText}' message in chat {chatId}.");

            AddChatUser(chatId, BotState.HANDLE_COMMAND);

            if (messageText == "/cancel")
            {
                ChangeState(BotChatList, BotState.HANDLE_COMMAND, chatId);

                await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "Операция отменена",
                           parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                           cancellationToken: cancellationToken
                           );
                return;
            }

            if (messageText.StartsWith("/broadcast"))
            {
                var user = await userHelper.DBUSer(chatId);
                var array = messageText.Split(" ");
                string message = "";
                
                if (user.Role.Id == 1)
                {
                    await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "У вас недостаточно прав для этой команды",
                           parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                           cancellationToken: cancellationToken
                           );

                    return;
                }

                if (array.Length < 3)
                {
                    await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "Неверный формат команды",
                           parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                           cancellationToken: cancellationToken
                           );

                    return;
                }

                if (!(int.TryParse(array[1], out var number) || array[1] == "*"))
                {
                    await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "Неверный формат навправления",
                           parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                           cancellationToken: cancellationToken
                           );

                    return;
                } 

                for (int i = 2; i < array.Length; i++)
                {
                    message += array[i] + " ";
                }

                if (array[1] == "*")
                {
                    if (user.Role.Id == 3)
                    {
                        foreach (var item in await userHelper.GetAllUsersAsync())
                        {
                            await botClient.SendTextMessageAsync(
                               chatId: item.TelegramId,
                               text: message,
                               parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                               cancellationToken: cancellationToken
                               );   
                        }
                        return;
                    }

                    else
                    {
                        await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "У вас недостаточно прав для этой команды",
                           parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                           cancellationToken: cancellationToken
                           );
                    }
                    return;
                } 

                var userList = broadcastHelper.GetUserByDir(Convert.ToInt32(array[1]));

                foreach (var item in userList)
                {
                    await botClient.SendTextMessageAsync(
                            chatId: item.TelegramId,
                            text: message,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                            cancellationToken: cancellationToken
                            );
                }
                await LoggerSinglton.GetFileManager().WriteToFile(LoggerSinglton.FileInfo, $"User {chatId} sent a broadcast: {message}");

                return;
            }

            switch (BotChatList.FirstOrDefault(b => b.ChatID == chatId).State)
            {
                case BotState.HANDLE_REGISTER_ANSWER:
                    var result = regHelper.Registration(messageText, chatId);
                    if (result == false)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Неверный формат ФИО или вы уже зарегистрированы. Попробуйте еще раз.",
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
                        new KeyboardButton[] {
                            "Мобильная разработка"
                        },
                        new KeyboardButton[] {
                            "Разработка desktop-приложений"
                        },
                        new KeyboardButton[] {
                            "Разработка комплексных ИС"
                        },
                    })
                    {
                        ResizeKeyboard = true,
                    };
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
                            replyMarkup: new ReplyKeyboardRemove(),
                            text: "Произошла ошибка, попробуйте еще раз.",
                            cancellationToken: cancellationToken
                            ) ;
                        ChangeState(BotChatList, BotState.HANDLE_REGISTER_COURSE, chatId);
                        return;
                    }
                    ReplyKeyboardMarkup replyKeyboardConfirm = new(new[] {
                        new KeyboardButton[]{
                            "Да",
                            "Нет"
                        },
                    })
                    {
                        ResizeKeyboard = true,
                    };
                    await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: $"Вы {regHelper.User.SecondName} {regHelper.User.FirstName} {regHelper.User.LastName} из группы {regHelper.User.GroupByCollege.Name}?",
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
                           replyMarkup: new ReplyKeyboardRemove(),
                           text: "Вы успешно зарегистрировались!",
                           cancellationToken: cancellationToken
                           );

                        await LoggerSinglton.GetFileManager().WriteToFile(LoggerSinglton.FileInfo, $"User {chatId} successfully registered");

                        ChangeState(BotChatList, BotState.HANDLE_COMMAND, chatId);
                    }
                    if (messageText == "Нет")
                    {
                        await regHelper.SaveUser();
                        await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           replyMarkup: new ReplyKeyboardRemove(),
                           text: "Регистрация отменена",
                           cancellationToken: cancellationToken
                           );
                        await LoggerSinglton.GetFileManager().WriteToFile(LoggerSinglton.FileInfo, $"User {chatId} canceled registration");

                        ChangeState(BotChatList, BotState.HANDLE_COMMAND, chatId);
                    }
                    return;
                case BotState.HANDLE_CHANGE_FIO:
                    var userRefresh = await userHelper.RefreshUser(chatId, messageText);
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                       text: $"*Ваше новое ФИО*: {userRefresh.SecondName} {userRefresh.FirstName} {userRefresh.LastName}",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                        cancellationToken: cancellationToken
                        );
                    await LoggerSinglton.GetFileManager().WriteToFile(LoggerSinglton.FileInfo, $"User {chatId} changed credentials");

                    ChangeState(BotChatList, BotState.HANDLE_COMMAND, chatId);
                    return;
                default:
                    break;
            }

            if (BotChatList.FirstOrDefault(b => b.ChatID == chatId).State is BotState.HANDLE_COMMAND)
            {
                switch (messageText)
                {
                    case "/start":
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Гильдия программистов *CodeHub*\n",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                            cancellationToken: cancellationToken
                            );

                        ChangeState(BotChatList, BotState.HANDLE_COMMAND, chatId);
                        break;
                    case "/about":
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Бот разработан @youveryaggressiveman и @grakhov",
                            cancellationToken: cancellationToken
                            );
                        ChangeState(BotChatList, BotState.HANDLE_COMMAND, chatId);
                        break;
                    case "/reg":
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Введите ваше ФИО (разделив одним пробелом): ",
                            cancellationToken: cancellationToken
                            );
                        ChangeState(BotChatList, BotState.HANDLE_REGISTER_ANSWER, chatId);
                        break;
                    case "/userinfo":
                        var user = await userHelper.DBUSer(chatId);
                        string directions = "";
                        string head = "";

                        foreach (var item in user.GroupByGuildOfUsers)
                        {
                            directions += item.GroupByGuild.Name + ";";

                            foreach (var itemHead in item.GroupByGuild.HeadOfGroups)
                            {
                                head += itemHead.Head.SecondName + " " + itemHead.Head.FirstName + " " + itemHead.Head.LastName + ";";
                            }
                        }

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"*Ваше ФИО*: {user.SecondName} {user.FirstName} {user.LastName}\n" +
                            $"*Ваше направление*: {directions}\n" +
                            $"*Ваши руководители*: {head}",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                            cancellationToken: cancellationToken
                            );
                        ChangeState(BotChatList, BotState.HANDLE_COMMAND, chatId);
                        break;
                    case "/change":
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Введите вашу ФИО (разделив одним пробелом) для изменения: ",
                            cancellationToken: cancellationToken
                            );
                        ChangeState(BotChatList, BotState.HANDLE_CHANGE_FIO, chatId);
                       
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

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);

            await LoggerSinglton.GetFileManager().WriteToFile(LoggerSinglton.FileInfo, ErrorMessage);

        }
    }
}

