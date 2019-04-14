using Lidgren.Network;
using System;

namespace AkroNetworking {
    public class ServerFrame {
        private NetServer _server;

        public IMessageReceiver Receiver { get; }
        public PersistentNetworkConnector Connector { get; }
        public MessageSender Sender { get; }
        public IAuthenticator Authenticator { get; }
        public bool Running { get; private set; }

        public ServerFrame(NetPeerConfiguration config,
                           IMessageReceiver receiver,
                           MessageSender sender,
                           PersistentNetworkConnector connector,
                           IAuthenticator authenticator) {

            _server = new NetServer(config);

            Receiver = receiver;
            Sender = sender;
            Connector = connector;
            Authenticator = authenticator;

            Sender.Sent += Sender_Send;
            Sender.Broadcast += Sender_Broadcast;
        }

        private void Sender_Send(object sender, Packets.Packet packet) {
            NetOutgoingMessage msg = _server.CreateMessage(4);
            packet.Write(msg);
            _server.SendMessage(msg, Connector.GetConnection(packet.Connection), packet.Method.ToNetDeliveryMethod());
        }
        private void Sender_Broadcast(Packets.Packet packet, long[] recipients) {
            var connections = new System.Collections.Generic.List<NetConnection>(recipients.Length);
            foreach(var rec in recipients) {
                var con = Connector.GetConnection(rec);
                if(con != null) {
                    connections.Add(con);
                }
            }
            NetOutgoingMessage msg = _server.CreateMessage(4);
            packet.Write(msg);
            _server.SendMessage(msg, connections, packet.Method.ToNetDeliveryMethod(), 0);
        }

        public void StartListen() {
            if (!Running) {
                Console.WriteLine("Server Started");
                Running = true;
                _server.Start();
            }
        }

        public void Close() {
            _server.Shutdown("Server shutting down");
            Running = false;
        }
        public void ReceiveSome(int limit) {
            NetIncomingMessage msg = null;
            while (limit-- > 0 && (msg = _server.ReadMessage()) != null) {
                switch (msg.MessageType) {
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        Console.WriteLine($"[SERVER]: {msg.ReadString()}");
                        break;
                    case NetIncomingMessageType.ConnectionApproval: {
                            var status = AuthenticationStatus.Unknown;
                            status = Authenticator.Authenticate(msg, out UserHandle handle);
                            if (status == AuthenticationStatus.OK) {
                                Connector.OnAuthenticated(msg.SenderConnection, handle);
                                msg.SenderConnection.Approve();
                            } else {
                                msg.SenderConnection.Deny(((int)status).ToString());
                            }
                        }
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        Console.WriteLine($"[SERVER]: {msg.ReadString()}");
                        break;
                    case NetIncomingMessageType.StatusChanged: {
                            var status = (NetConnectionStatus)msg.ReadByte();
                            var message = msg.LengthBytes > 0 ? msg.ReadString() : "";
                            switch (status) {
                                case NetConnectionStatus.Connected:
                                    Connector.OnConnected(msg.SenderConnection, message);
                                    break;
                                case NetConnectionStatus.Disconnected:
                                    Connector.OnDisconnected(msg.SenderConnection, message);
                                    break;
                            }
                        }
                        break;
                    case NetIncomingMessageType.Data:
                        Receiver.DecodePacket(msg);
                        break;
                }
                _server.Recycle(msg);
            }
        }
        public void DispatchEvents() {
            Connector.DispatchEvents();
        }
    }

    public static class ServerFrameFactory {
        public static ServerFrame Create(string id, int timeout, int ping, int port, IMessageReceiver receiver, IAuthenticator authenticator, PersistentNetworkConnector connector) {
            NetPeerConfiguration config = new NetPeerConfiguration(id) {
                PingInterval = ping,
                ConnectionTimeout = timeout,
                Port = port,
                UseMessageRecycling = true,
                SendBufferSize = 134_216_704,
                ReceiveBufferSize = 134_216_704,
                MaximumConnections = 100,
                MaximumHandshakeAttempts = 100
            };
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

#if DEBUG
            config.SimulatedMinimumLatency = 0.015f;
            config.SimulatedRandomLatency = 0.010f;
            config.SimulatedDuplicatesChance = 0.01f;
            config.SimulatedLoss = 0.01f;
#endif // DEBUG

            var sender = MessageSenderFactory.CreateMessageSender();

            ServerFrame sf = new ServerFrame(config, receiver, sender, connector, authenticator);
            return sf;
        }
    }
}
