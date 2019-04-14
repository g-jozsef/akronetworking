using System;
using Lidgren.Network;

namespace AkroNetworking {
    class Authenticator : IAuthenticator {
        public event EventHandler<UserHandle> Authenticated;

        public AuthenticationStatus Authenticate(NetIncomingMessage msg, out UserHandle handle) {
            handle = new UserHandle() {
                UserID = 0,
                ConnectionID = msg.SenderConnection.RemoteUniqueIdentifier
            };
            Authenticated?.Invoke(this, handle);
            return AuthenticationStatus.OK;
        }
    }
}
