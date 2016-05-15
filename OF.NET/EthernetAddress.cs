using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.NetworkInformation
{
    public static class EthernetAddress
    {
        public static readonly PhysicalAddress NDP_MULTICAST =
            new PhysicalAddress(new byte[] {0x01, 0x23, 0x20, 0x00, 0x00, 0x01});

        public static readonly PhysicalAddress LLDP_MULTICAST =
            new PhysicalAddress(new byte[] {0x01, 0x80, 0xc2, 0x00, 0x00, 0x0e});

        public static readonly PhysicalAddress ETHER_BROADCAST =
            new PhysicalAddress(new byte[] {0xff, 0xff, 0xff, 0xff, 0xff, 0xff});

        public static readonly PhysicalAddress BRIDGE_GROUP_ADDRESS =
            new PhysicalAddress(new byte[] {0x01, 0x80, 0xc2, 0x00, 0x00, 0x00});

        public static readonly PhysicalAddress PAE_MULTICAST =
            new PhysicalAddress(new byte[] {0x01, 0x80, 0xc2, 0x00, 0x00, 0x03});

        public static readonly PhysicalAddress ETHER_ANY =
            new PhysicalAddress(new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00});

    }
}
