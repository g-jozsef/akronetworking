using Lidgren.Network;

namespace AkroNetworking {
    public interface IMessageReceiver {
        void DecodePacket(NetIncomingMessage msg);
    }
}