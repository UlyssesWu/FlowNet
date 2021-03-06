﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Be.IO;

namespace FlowNet.OpenFlow
{
    public static class Helper
    {
        public static PhysicalAddress ToPhysicalAddress(this byte[] bytes)
        {
            if (bytes.Length!=6)
            {
                return PhysicalAddress.None;
            }
            return new PhysicalAddress(bytes);
        }
    }
}
