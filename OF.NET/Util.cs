using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using MiscUtil.Conversion;

namespace FlowNet
{
    public static class Util
    {
        public static bool IsAvailable(this IPAddress ip)
        {
            return !Equals(ip, IPAddress.Any) && !Equals(ip, IPAddress.Broadcast) && !Equals(ip, IPAddress.None);
        }
        public static ulong GetDatapathId(this PhysicalAddress mac)
        {
            try
            {
                return EndianBitConverter.Big.ToUInt64(new byte[] {0x00, 0x00}.Concat(mac.GetAddressBytes()).ToArray(), 0);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static PhysicalAddress GetPhysicalAddress(this ulong dpid)
        {
            try
            {
                return new PhysicalAddress(EndianBitConverter.Big.GetBytes(dpid).Skip(2).ToArray());
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string ToSwitchString(this ulong dpid)
        {
            return $"s-{dpid.ToString("X")}";
        }

        public static ulong ToSwitchDpid(this string s)
        {
            return ulong.Parse(s.Substring(2), NumberStyles.AllowHexSpecifier);
        }

        public static string ToHostString(this PhysicalAddress mac)
        {
            return $"h-{mac}";
        }

        public static PhysicalAddress ToHostMacAddress(this string s)
        {
            return PhysicalAddress.Parse(s.Substring(2));
        }

    }
}
