using Lidgren.Network;
using P = AkroNetworking.Packets;
using System;
using System.Linq;
using System.Reflection;

namespace AkroNetworking {
    public static class MessageReceiverFactory {
        public static NoOpMessageReceiver CreateNoOpMessageReceiver() {
            return new NoOpMessageReceiver();
        }
    }
    public class NoOpMessageReceiver : IMessageReceiver {
        public virtual void DecodePacket(NetIncomingMessage msg) {
        }
    }
}
