using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using PuppetMaster;

namespace PuppetMaster
{
    class Command_Delay : Command
    {
        int delay;

        public Command_Delay(Controller c, int delay)
            : base(c)
        {
            this.delay = delay;
        }

        public override bool Send(CommBuffer buffer, out bool send, out bool free)
        {
            Thread.Sleep(delay);

            send = false;
            free = true;
            return true;
        }

        public static Command Parse(Controller c, string line)
        {
            string[] split = line.Split(' ');

            return new Command_Delay(c, Convert.ToInt32(split[1]));
        }
    }
}
