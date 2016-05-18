using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace PuppetMaster
{
    public class State_Idle : State
    {
        public State_Idle(Controller c)
            : base(c)
        {
        }

        public override string Name()
        {
            return "Idle";
        }

        public override State DoStuff()
        {
            Command_StopAll comm = new Command_StopAll(c);

            Robot r = c.r;
            SerialComm serialComm = r.comm;
            CommBuffer buffer = serialComm.outputBuffer;

            bool send, free;
            comm.Send(buffer, out send, out free);

            if (!c.enginesOff)
            {
                //serialComm.ForceSendMessage();
                serialComm.SendMessage();
                c.enginesOff = true;
            }

            if (c.r.smallBot) return this;
            if (c.isUmbrellaDeployed || c.running) return this;

            c.isUmbrellaDeployed = true;
            return new State_Parser(c, "otvoriKisobran.servo");
        }
    }
}
