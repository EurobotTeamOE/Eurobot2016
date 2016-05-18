using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace PuppetMaster
{
    public abstract class Command
    {
        public enum Arm
        {
            Left,
            Right,
            Cup
        }

        protected byte ID;
        protected Controller c;

        public Command(Controller c)
        {
            this.c = c;
            SerialComm comm = c.r.comm;
            ID = comm.NextCommandID();
        }

        public virtual bool Send(CommBuffer buffer, out bool send, out bool free)
        {
            c.r.upToDate = 0;
            send = true;
            free = true;

            return true;
        }
    }
}
