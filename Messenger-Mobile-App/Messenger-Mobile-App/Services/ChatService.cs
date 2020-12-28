using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

namespace Messenger_Mobile_App.Services
{
    public class ChatService
    {
        string connectionUrl = "http://10.0.2.2:49499/messagehub";
        HubConnection connection; 
        
        public ChatService()
        {
            connection = new HubConnectionBuilder().WithUrl(connectionUrl).Build();
        }

        public async Task Connect()
        {
            if (connection.State == HubConnectionState.Connected) return;

            await connection.StartAsync();
        }

        public async Task Disconnect()
        {
            await connection.DisposeAsync();
        }
    }
}
