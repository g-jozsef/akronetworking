using System;
using System.Collections.Generic;
using System.Text;

namespace AkroNetworking {
    public struct UserHandle {
        public uint UserID { get; set; }
        public long ConnectionID { get; set; }

        public static bool operator ==(UserHandle a, UserHandle b) {
            return a.UserID == b.UserID && a.ConnectionID == b.ConnectionID;
        }
        public static bool operator !=(UserHandle a, UserHandle b) {
            return !(a == b);
        }

        public override bool Equals(object obj) {
            if (!(obj is UserHandle)) {
                return false;
            }

            var handle = (UserHandle)obj;
            return UserID == handle.UserID &&
                   ConnectionID == handle.ConnectionID;
        }

        public override int GetHashCode() {
            var hashCode = 1935117043;
            hashCode = hashCode * -1521134295 + UserID.GetHashCode();
            hashCode = hashCode * -1521134295 + ConnectionID.GetHashCode();
            return hashCode;
        }
    }
}
