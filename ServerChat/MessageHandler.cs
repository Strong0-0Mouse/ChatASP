using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using ServerChat.Hubs;

namespace ServerChat
{
    public class MessageHandler : IHostedService
    {
        private readonly IHubContext<ChatHub, IChatHub> _hubContext;

        public MessageHandler(IHubContext<ChatHub, IChatHub> hubContext)
        {
            _hubContext = hubContext;
            ChatHub.OnMessageReceive += OnMessageReceive;
            ChatHub.OnErrorConnected += OnErrorConnected;
            ChatHub.OnCommandReceive += OnCommandReceive;
        }

        private void OnCommandReceive(string commandResult, string user)
        {
            _hubContext.Clients.Client(user).SendFromServer(commandResult);
        }

        private void OnErrorConnected(int code, string user)
        {
            _hubContext.Clients.Client(user).ErrorConnectedClient(code);
        }

        private void OnMessageReceive(string message)
        {
            _hubContext.Clients.All.SendFromServer(message);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}