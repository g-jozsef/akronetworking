using Lidgren.Network;

namespace AkroNetworking {
    public enum DeliveryMethod : byte {
        Unknown = 0,
        Reliable = 1,
        Unreliable = 2,
        UnreliableOrdered = 3
    }

    public static class NetDeliveryMethodExtensions {
        public static NetDeliveryMethod ToNetDeliveryMethod(this DeliveryMethod value) {
            switch (value) {
                case DeliveryMethod.Reliable:
                    return NetDeliveryMethod.ReliableOrdered;
                case DeliveryMethod.Unreliable:
                    return NetDeliveryMethod.Unreliable;
                case DeliveryMethod.UnreliableOrdered:
                    return NetDeliveryMethod.UnreliableSequenced;
                default:
                    return NetDeliveryMethod.Unknown;
            }
        }
    }
    public static class DeliveryMethodExtensions {
        public static DeliveryMethod ToDeliveryMethod(this NetDeliveryMethod value) {
            switch (value) {
                case NetDeliveryMethod.ReliableOrdered:
                    return DeliveryMethod.Reliable;
                case NetDeliveryMethod.Unreliable:
                    return DeliveryMethod.Unreliable;
                case NetDeliveryMethod.UnreliableSequenced:
                    return DeliveryMethod.UnreliableOrdered;
                default:
                    return DeliveryMethod.Unknown;
            }
        }
    }
}
