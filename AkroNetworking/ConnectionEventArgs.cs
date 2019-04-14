using System;
using System.Collections.Generic;
using System.Text;

namespace AkroNetworking {
    public class ConnectionEventArgs : EventArgs {

        public UserHandle Sender { get; }
        public String Data { get; }

        public ConnectionEventArgs(UserHandle sender, string data = "") {
            Data = data;
            Sender = sender;
        }
    }
}
