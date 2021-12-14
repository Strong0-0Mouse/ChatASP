using System.Threading.Tasks;

namespace ServerChat.Hubs
{
    public interface IChatHub
    {
        Task SendFromServer(string message);
        Task SendFromClient(string name, string message);
        Task ErrorConnectedClient(int errorCode);
        Task ConnectedClient(string name, string password);
        Task RegistrationClient(string name, string password);
        Task DisconnectedClient(string name);
        Task CommandMessage(string command);
        Task PersonalMessage(string nameTo, string nameFrom, string message);
    }
}