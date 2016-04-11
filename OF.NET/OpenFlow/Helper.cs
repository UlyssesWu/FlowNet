using System;
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

        public static void Test()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var bw = new BeBinaryWriter(ms);
                bw.Write((ulong)1);
                bw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                var br = new BeBinaryReader(ms);
                var ans = br.ReadInt64();
            }
        }
    }
}
