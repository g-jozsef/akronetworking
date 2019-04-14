using System;
using System.Collections.Generic;
using System.Text;
using AkroNetworking.Packets;
using Lidgren.Network;

namespace AkroNetworking {
    public class NoAuthenticator : IAuthenticator {
        public AuthenticationStatus Authenticate(NetIncomingMessage msg, out UserHandle handle) {
            handle = default(UserHandle);
            return AuthenticationStatus.OK;
        }
    }
}
