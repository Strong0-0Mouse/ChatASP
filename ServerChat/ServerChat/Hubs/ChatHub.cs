using System.Linq;
using Microsoft.AspNetCore.SignalR;
using ServerChat.Models;

namespace ServerChat.Hubs
{
    public class ChatHub : Hub<IChatHub>
    {
        private readonly Logger _logger;
        private readonly UserDbContext _userDbContext;
        public delegate void MessageHandler(string message);
        public static event MessageHandler OnMessageReceive;
        
        public delegate void MessageErrorHandler(int code, string userId);
        public static event MessageErrorHandler OnErrorConnected;
        
        public delegate void CommandHandler(string commandResult, string userId);
        public static event CommandHandler OnCommandReceive;

        public ChatHub(Logger logger, UserDbContext userDbContext)
        {
            _logger = logger;
            _userDbContext = userDbContext;
        }

        public void SendFromClient(string name, string message)
        {
            OnMessageReceive?.Invoke($"{name}: {message}");
            _logger.Log($"{name}: {message}");
        }

        public void ConnectedClient(string name, string password)
        {
            if (_userDbContext.Users.Any(o => o.Name == name))
            {
                if (_userDbContext.Users.Any(o => o.Password == password))
                {
                    OnErrorConnected?.Invoke(0, Context.ConnectionId);
                    OnMessageReceive?.Invoke($">> {name} присоединился <<");
                    _userDbContext.UsersOnline.Add(new UserOnline {Name = name, ConnectionId = Context.ConnectionId});
                    _userDbContext.SaveChanges();
                    _logger.Log($">> \"{name}\" присоединился <<");
                }
                else
                {
                    OnErrorConnected?.Invoke(-1, Context.ConnectionId);
                    _logger.Log($">> \"{name}\" не верно введен пароль <<");
                }
            }
            else
            {
                OnErrorConnected?.Invoke(-2, Context.ConnectionId);
                _logger.Log($">> Пользователь \"{name}\" не найден <<");
            }
        }

        public void RegistrationClient(string name, string password)
        {
            if (_userDbContext.Users.Any(o => o.Name == name))
            {
                OnErrorConnected?.Invoke(-3, Context.ConnectionId);
            }
            else
            {
                _userDbContext.Users.Add(new User { Name = name, Password = password});
                _userDbContext.UsersOnline.Add(new UserOnline {Name = name, ConnectionId = Context.ConnectionId});
                _userDbContext.SaveChanges();
                OnErrorConnected?.Invoke(0, Context.ConnectionId);
                OnMessageReceive?.Invoke($">> {name} зарегистрировался <<");
                _logger.Log($">> \"{name}\" зарегистрировался и присоединился <<");
            }
        }

        public void CommandMessage(string command)
        {
            var result = string.Empty;
            switch (command.ToLower())
            {
                case "users":
                    var users = _userDbContext.Users.Select(p => p.Name).ToList();
                    foreach (var user in users)
                        result += $"| {user} |";
                    break;
                case "online":
                    var usersOnline = _userDbContext.UsersOnline.Select(p => p.Name).ToList();
                    foreach (var user in usersOnline)
                        result += $"| {user} |";
                    break;
                case "help":
                    result = "users: Вывести список всех пользователей | online: Список пользователей онлайн";
                    break;
            }
            OnCommandReceive?.Invoke(result, Context.ConnectionId);
        }
        
        public void PersonalMessage(string nameTo, string nameFrom, string message)
        {
            var users = _userDbContext.UsersOnline
                .Select(p => new {p.Name, p.ConnectionId})
                .Where(u => u.Name == nameTo)
                .ToList();
            foreach (var user in users)
                OnCommandReceive?.Invoke($"(pm) {nameFrom}: {message}", user.ConnectionId);
        }

        public void DisconnectedClient(string name)
        {
            var user = _userDbContext.UsersOnline
                .FirstOrDefault(o => o.ConnectionId == Context.ConnectionId);
            if (user == null) return;
            _userDbContext.UsersOnline.Remove(user!);
            _userDbContext.SaveChanges();
            OnMessageReceive?.Invoke($">> {name} вышел <<");
            _logger.Log($">> \"{name}\" вышел <<");
        }
    }
}