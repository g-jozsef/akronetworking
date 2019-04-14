using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AkroNetworking {

    public static class PersistentNetworkConnectorFactory {
        public static PersistentNetworkConnector CreatePersistentNetworkConnector() {
            return new PersistentNetworkConnector();
        }
    }
    public class PersistentNetworkConnector {
        private Queue<Action> _awaitingActions;
        private Dictionary<UserHandle, NetConnection> _connections;
        public Dictionary<long, UserHandle> Handles { get; }

        /// <summary>
        /// Authenticated is called when the connection has been authenticated, but before the final Approve call
        /// </summary>
        public event EventHandler<ConnectionEventArgs> Authenticated;
        /// <summary>
        /// Connected is called after the client is connected
        /// </summary>
        public event EventHandler<ConnectionEventArgs> Connected;
        /// <summary>
        /// Disconnected is called before the client is disconnected
        /// </summary>
        public event EventHandler<ConnectionEventArgs> Disconnected;

        public PersistentNetworkConnector() {
            _awaitingActions = new Queue<Action>();
            _connections = new Dictionary<UserHandle, NetConnection>();
            Handles = new Dictionary<long, UserHandle>();
        }

        internal void OnConnected(NetConnection con, String message) {
            _awaitingActions.Enqueue(() => {
                var uID = con.RemoteUniqueIdentifier;
                Connected?.Invoke(this, new ConnectionEventArgs(Handles[uID]));
            });
        }
        internal void OnDisconnected(NetConnection con, String message) {
            _awaitingActions.Enqueue(() => {
                var uID = con.RemoteUniqueIdentifier;
                if (Handles.ContainsKey(uID)) {
                    Disconnected?.Invoke(this, new ConnectionEventArgs(Handles[uID], message));
                    _connections.Remove(Handles[uID]);
                }
            });
        }
        internal void OnAuthenticated(NetConnection con, UserHandle handle) {
            _awaitingActions.Enqueue(() => {
                var uID = con.RemoteUniqueIdentifier;
                Handles[uID] = handle;
                _connections.Add(Handles[uID], con);
                Authenticated?.Invoke(this, new ConnectionEventArgs(Handles[uID]));
            });
        }

        internal NetConnection GetConnection(long handle) {
            if (Handles.ContainsKey(handle))
                return _connections.ContainsKey(Handles[handle]) ? _connections[Handles[handle]] : null;
            else
                return null;
        }

        public void DispatchEvents() {
            while (_awaitingActions.Any()) {
                var action = _awaitingActions.Dequeue();
                action?.Invoke();
            }
        }
    }
}
