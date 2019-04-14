using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkroNetworking {
    public interface IAuthenticator {
        AuthenticationStatus Authenticate(NetIncomingMessage msg, out UserHandle handle);
    }
}
