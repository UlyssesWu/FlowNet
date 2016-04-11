using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public class BinaryWriterBE : BinaryWriter
    {
        public BinaryWriterBE(Stream output) : base(output)
        {
        }

        public override void Write(int value)
        {
            base.Write(value);
        }
    }
}
