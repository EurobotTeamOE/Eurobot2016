using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace PuppetMaster
{
    public class Command_StopAll : Command
    {
        public Command_StopAll(Controller c)
            : base(c)
        {
        }

        public override bool Send(CommBuffer buffer, out bool send, out bool free)
        {
            buffer.Write(0xFF);
            buffer.Write(0x0A);
            buffer.Write(2);

            buffer.Write((byte)0x0A);
            //buffer.Write((byte)0x00);

            //buffer.Write((byte)(ID & 0x0F));
            //buffer.Write((byte)((ID & 0xF0) >> 4));

            return base.Send(buffer, out send, out free);
        }
    }
}
