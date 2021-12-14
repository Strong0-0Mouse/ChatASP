using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace ClientChat
{
    internal static class Program
    {
        private static HubConnection _hubConnection;
        private static int PositionCursor { get; set; } = 1;
        private static bool IsLogin { get; set; }
        private static bool IsEndMessageFromServer { get; set; }
        private static string Name { get; set; }

        public static async Task Main()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
            _hubConnection = new HubConnectionBuilder().WithUrl("http://172.17.3.217:5000").Build();
            _hubConnection.On<string>("SendFromServer", OutputMessageFromServer);
            _hubConnection.On<int>("ErrorConnectedClient", OutputErrorFromServer);
            await _hubConnection.StartAsync();
            while (IsLogin == false)
            {
                Console.WriteLine("1 - Вход\n2 - Регистрация");
                var numAction = Console.ReadLine();
                switch (numAction)
                {
                    case "1":
                        await ConnectedHandler();
                        break;
                    case "2":
                        await RegistrationHandler();
                        break;
                }
                Console.Clear();
            }
            while (true)
            {
                Console.Write("> ");
                var message = Console.ReadLine();
                ClearLine(1);
                if (message!.Contains("/pm"))
                {
                    var resultMessage = string.Empty;
                    var messageArray = message.Replace("/pm", "").Trim().Split(" ");
                    for (var i = 1; i < messageArray.Length; i++)
                        resultMessage += $"{messageArray[i]} ";
                    if (messageArray.Length > 2)
                        await _hubConnection.SendAsync("PersonalMessage", messageArray[0], Name,
                            resultMessage.Trim());
                }
                else if (message[0] == '/')
                    await _hubConnection.SendAsync("CommandMessage", message.Trim(new[] {' ', '/'}));
                else
                    await _hubConnection.SendAsync("SendFromClient", Name, message);
            }
        }
        private static async Task ConnectedHandler()
        {
            while (IsLogin == false)
            {
                IsEndMessageFromServer = false;
                Console.Write("Логин: ");
                Name = Console.ReadLine();
                Console.Write("Пароль: ");
                var password = Console.ReadLine();
                await _hubConnection.SendAsync("ConnectedClient", Name, password);
                while (IsEndMessageFromServer == false) { }
            }
        }
        private static async Task RegistrationHandler()
        {
            while (IsLogin == false)
            {
                IsEndMessageFromServer = false;
                Console.Write("Логин: ");
                Name = Console.ReadLine();
                Console.Write("Пароль: ");
                var password = Console.ReadLine();
                await _hubConnection.SendAsync("RegistrationClient", Name, password);
                while (IsEndMessageFromServer == false) { }
            }
        }
        private static void OutputErrorFromServer(int code)
        {
            switch (code)
            {
                case 0:
                    IsLogin = true;
                    Console.Clear();
                    break;
                case -1:
                    Console.Clear();
                    IsLogin = false;
                    Console.WriteLine("Неверный пароль");
                    break;
                case -2:
                    Console.Clear();
                    IsLogin = false;
                    Console.WriteLine("Пользователь с таким логином не найден");
                    break;
                case -3:
                    Console.Clear();
                    IsLogin = false;
                    Console.WriteLine("Пользователь с таким логином уже существует");
                    break;
            }
            IsEndMessageFromServer = true;
        }
        private static void OutputMessageFromServer(string message)
        {
            if (IsLogin)
            {
                var positionX = Console.CursorLeft;
                if (positionX == 0)
                    positionX += 2;
                var positionY = Console.CursorTop;
                Console.SetCursorPosition(0, PositionCursor);
                Console.Write(message);
                Console.SetCursorPosition(positionX, positionY);
                PositionCursor++;
            }
        }
        private static void ClearLine(int numLine)
        {
            for (int i = 0; i < numLine; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.BufferWidth));
            }
            Console.SetCursorPosition(0, 0);
        }
        static async void ProcessExit(object sender, EventArgs e)
        {
            await _hubConnection.SendAsync("DisconnectedClient", Name);
        }
    }
}