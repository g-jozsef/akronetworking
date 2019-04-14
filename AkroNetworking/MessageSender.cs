using Lidgren.Network;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace AkroNetworking {
    public static class MessageSenderFactory {
        public static MessageSender CreateMessageSender() {
            return new MessageSender();
        }
    }
    public class MessageSender {
        public delegate void BroadcastDelegate(Packets.Packet pakcet, long[] recipients);
        public event EventHandler<Packets.Packet> Sent;
        public event BroadcastDelegate Broadcast;

        public void BroadcastP(IEnumerable<UserHandle> handles, Packets.Packet p) {
            if (handles.Any())
                Broadcast?.Invoke(p, handles.Select(x => x.ConnectionID).ToArray());
        }

        public void Send(UserHandle handle, Packets.Packet p) {
            p.Connection = handle.ConnectionID;
            Sent?.Invoke(this, p);
        }
        public void Send(Int64 recipient, Packets.Packet p) {
            p.Connection = recipient;
            Sent?.Invoke(this, p);
        }
        public void Send(Packets.Packet p) {
            Sent?.Invoke(this, p);
        }
    }
}
