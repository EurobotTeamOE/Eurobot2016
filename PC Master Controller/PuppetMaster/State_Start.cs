using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class State_Start : State
    {
        int timeout;

        public State_Start(Controller c, int timeout)
            : base(c)
        {
            this.timeout = timeout;
        }

        public override string Name()
        {
            return "Start";
        }

        public override State DoStuff()
        {
            if (c.time.TotalMilliseconds > timeout) return c.GetNextState();
            return this;
        }

        public static State Parse(Controller c, string line)
        {
            string[] split = line.Split(' ');
            return new State_Start(c, Convert.ToInt32(split[1]));
        }
    }
}
