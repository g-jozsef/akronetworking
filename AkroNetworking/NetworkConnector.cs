using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AkroNetworking {
    public static class NetworkConnectorFactory {
        public static NetworkConnector CreateNetworkConnector() {
            return new NetworkConnector();
        }
    }
    public class NetworkConnector {
        private Queue<Action> _awaitingActions;
        public event EventHandler<ConnectionEventArgs> Connected;
        public event EventHandler<ConnectionEventArgs> Disconnected;

        public NetworkConnector() {
            _awaitingActions = new Queue<Action>();
        }

        public void OnConnected(NetConnection con, String message) {
            _awaitingActions.Enqueue(() => {
                Connected?.Invoke(this, new ConnectionEventArgs(default(UserHandle), message));
            });
        }
        public void OnDisconnected(NetConnection con, String message) {
            _awaitingActions.Enqueue(() => {
                Disconnected?.Invoke(this, new ConnectionEventArgs(default(UserHandle), message));
            });
        }

        public void DispatchEvents() {
            while (_awaitingActions.Any()) {
                var action = _awaitingActions.Dequeue();
                action?.Invoke();
            }
        }
    }
}
