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
        private static List<Group> _Groups = new List<Group>();

        public override Task OnConnected()
        {
            return Clients.Client(Context.ConnectionId).SetConnectionId(Context.ConnectionId);
        }

        public void GetConnectionId()
        {
            Clients.Caller.getConnectionId(Context.ConnectionId);
        }

        public async Task FindChat()
        {
            var availableRoom = _Groups.FirstOrDefault(p => string.IsNullOrEmpty(p.User2));
            if (availableRoom != null)
            {
                availableRoom.User2 = Context.ConnectionId;
                await Groups.Add(availableRoom.User1, availableRoom.GroupId);
                await Groups.Add(availableRoom.User2, availableRoom.GroupId);
                Clients.Group(availableRoom.GroupId).matchFound(availableRoom.GroupId);
            }
            else
            {
                var guid = Guid.NewGuid().ToString();
                _Groups.Add(new Group()
                {
                    GroupId = guid,
                    User1 = Context.ConnectionId
                });
                Clients.Caller.pending();
            }
        }

        public void SendMessage(string roomName, string message)
        {
            Clients.Group(roomName).receiveMessage(Context.ConnectionId, message);
        }

        public void Ping()
        {
            Clients.Caller.Ping();
        }

        public async Task DisconnectToRoom(string roomName)
        {
            var room = _Groups.FirstOrDefault(p => p.GroupId == roomName);
            if (room != null)
            {
                var user1 = room.User1;
                var user2 = room.User2;
                _Groups.Remove(room);
                Clients.Group(roomName).disconnect();
                await Groups.Remove(user1, roomName);
                await Groups.Remove(user2, roomName);
            }
        }
    }


    public class Group
    {
        public string GroupId { get; set; }

        public string User1 { get; set; }

        public string User2 { get; set; }
    }
}
