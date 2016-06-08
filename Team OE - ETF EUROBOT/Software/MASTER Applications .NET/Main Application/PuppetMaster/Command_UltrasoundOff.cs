using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using PuppetMaster;

namespace PuppetMaster
{
    class Command_UltrasoundOff : Command
    {
        public Command_UltrasoundOff(Controller c)
           :base(c)
        {

        }

        public override bool Send(CommBuffer buffer, out bool send, out bool free)
        {
            // Metadata
            buffer.Write(0xFF);
            buffer.Write(0x0A);
            buffer.Write(2);

            // Data
            buffer.Write((byte)(0x12));

            return base.Send(buffer, out send, out free);
        }

        public static Command Parse(Controller c, string line)
        {
            string[] split = line.Split(' ');

            return new Command_UltrasoundOff(c);
        }
    }
}
