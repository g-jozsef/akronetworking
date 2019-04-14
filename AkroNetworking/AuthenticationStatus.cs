using System;
using System.Collections.Generic;
using System.Text;

namespace AkroNetworking {
    public enum AuthenticationStatus {
        Unknown,
        OK,
        BadAuth,
        ConnectionLimit
    }
}
