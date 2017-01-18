using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmegleClone.Hubs
{
    [HubName("chatHub")]
    public class ChatHub : Hub
    {
        public override Task OnConnected()
        {
            return Clients.Client(Context.ConnectionId).SetConnectionId(Context.ConnectionId);
        }

        public void GetConnectionId()
        {
            Clients.Caller.getConnectionId(Context.ConnectionId);
        }

        public void FindChat()
        {
            Clients.All.LookForPartner(Context.ConnectionId);
        }

        public async Task MatchFound(string user1ConnId, string user2ConnId)
        {
            var guid = Guid.NewGuid().ToString();
            await Groups.Add(user1ConnId, guid);
            await Groups.Add(user2ConnId, guid);
            Clients.Group(guid).matchFound(guid);
        }

        public void SendMessage(string roomName, string message)
        {
            Clients.Group(roomName).receiveMessage(Context.ConnectionId, message);
        }

        public async Task DisconnectToRoom(string roomName, string user1ConnId, string user2ConnId)
        {
            Clients.Group(roomName).disconnect();
            await Groups.Remove(user1ConnId, roomName);
            await Groups.Remove(user2ConnId, roomName);
        }
    }
}
