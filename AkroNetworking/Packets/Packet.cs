using Lidgren.Network;
using System;

namespace AkroNetworking.Packets {

    public class Packet {
        public Int64 Connection { get; set; }
        public UInt32 OpCode { get; }
        public DeliveryMethod Method { get; }
        public int Tick { get; }

        public Packet(int tick, UInt32 opCode, DeliveryMethod method = DeliveryMethod.Reliable) {
            this.OpCode = opCode;
            this.Method = method;
            this.Tick = tick;
        }   
        public Packet(NetIncomingMessage msg) {
            Connection = msg.SenderConnection.RemoteUniqueIdentifier;
            Method = msg.DeliveryMethod.ToDeliveryMethod();
            OpCode = msg.ReadUInt32();
            Tick = msg.ReadInt32();
        }
        public virtual void Write(NetOutgoingMessage message) {
            message.Write(OpCode);
            message.Write(Tick);
        }

        public static implicit operator bool(Packet p) {
            return p != null;
        }
    }
}
