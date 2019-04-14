using Lidgren.Network;
using System;
using System.Threading;

namespace AkroNetworking {
    public class ClientFrame {
        private NetClient _client;
        public event EventHandler<float> ConnectionLatencyChanged;
        public IMessageReceiver Receiver { get; }
        public NetworkConnector Connector { get; }
        public bool Running { get; private set; }

        Thread _recvThread;

        public ClientFrame(NetPeerConfiguration config,
                           IMessageReceiver receiver,
                           NetworkConnector connector) {

            _client = new NetClient(config);
            Receiver = receiver;
            Connector = connector;
        }

        public void StartListen(bool newthread) {
            if (!Running) {
                Console.WriteLine("Client Started");
                Running = true;
                _client.Start();
                if (newthread) {
                    _recvThread = new Thread(new ThreadStart(ReceiveLoop));
                    _recvThread.Start();
                }
            }
        }

        public void Close() {
            _client.Shutdown("Client shutting down");
            Running = false;
        }
        public void Disconnect() {
            _client.Disconnect("");
        }
        public void WaitToFinish() {
            if (Running)
                _recvThread.Join();
        }
        public void Connect(string ip, int port, Packets.Packet p) {
            NetOutgoingMessage msg = _client.CreateMessage();
            p.Write(msg);
            _client.Connect(ip, port, msg);
        }
        public void Send(Packets.Packet i) {
            NetOutgoingMessage msg = _client.CreateMessage(4);
            i.Write(msg);
            _client.SendMessage(msg, i.Method.ToNetDeliveryMethod());
        }
        public bool ReceiveOne() {
            NetIncomingMessage msg = _client.ReadMessage();
            if (msg == null) return false;

            Console.WriteLine($"[CLIENT]: Got message of type: {msg.MessageType}");
            switch (msg.MessageType) {
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.ErrorMessage:
                    Console.WriteLine($"[CLIENT]: {msg.ReadString()}");
                    break;
                case NetIncomingMessageType.StatusChanged:
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
                    break;
                case NetIncomingMessageType.Data:
                    Receiver.DecodePacket(msg);
                    break;
                case NetIncomingMessageType.ConnectionLatencyUpdated:
                    float ping = msg.ReadSingle();
                    ConnectionLatencyChanged?.Invoke(this, ping);
                    break;
            }
            _client.Recycle(msg);

            return true;
        }
        public int ReceiveSome(int limit) {
            NetIncomingMessage msg = null;
            while (limit-- > 0 && (msg = _client.ReadMessage()) != null) {
                switch (msg.MessageType) {
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        break;
                    case NetIncomingMessageType.StatusChanged:
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
                        break;
                    case NetIncomingMessageType.Data:
                        Receiver.DecodePacket(msg);
                        break;
                    case NetIncomingMessageType.ConnectionLatencyUpdated:
                        float ping = msg.ReadSingle();
                        ConnectionLatencyChanged?.Invoke(this, ping);
                        break;
                }
                _client.Recycle(msg);
            }
            return limit;
        }
        private void ReceiveLoop() {
            while (Running) {
                _client.MessageReceivedEvent.WaitOne(2000);
                ReceiveOne();
            }
        }

        public void DispatchEvents() {
            Connector.DispatchEvents();
        }
    }

    public static class ClientFrameFactory {
        public static ClientFrame Create(string id, int timeout, int ping, IMessageReceiver receiver, NetworkConnector connector) {
            NetPeerConfiguration config = new NetPeerConfiguration(id);
            config.PingInterval = ping;
            config.ConnectionTimeout = timeout;
            config.UseMessageRecycling = true;
            config.SendBufferSize = 134_216_704;
            config.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);

#if DEBUG
            config.SimulatedMinimumLatency = 0.015f;
            config.SimulatedRandomLatency = 0.010f;
            config.SimulatedDuplicatesChance = 0.01f;
            config.SimulatedLoss = 0.01f;
#endif // DEBUG

            ClientFrame cf = new ClientFrame(config, receiver, connector);
            return cf;
        }
    }
}
