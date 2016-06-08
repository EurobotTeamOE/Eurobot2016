using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace PuppetMaster
{
    public class Command_MoveServo : Command
    {
        int servoID;
        int position;

        public Command_MoveServo(Controller c, int servoID, int position)
            : base(c)
        {
            this.servoID = servoID;
            this.position = position;
        }

        public override bool Send(CommBuffer buffer, out bool send, out bool free)
        {

            buffer.Write(0xFF);
            buffer.Write(0x07);
            buffer.Write(9);

            // Data
            buffer.Write((byte)(0x10));
            //buffer.Write((byte)(val >= 0 ? 0xF1 : 0xF2));
            //buffer.Write((byte)0x00);
            buffer.Write((byte)servoID);

            buffer.Write((byte)(position & 0x000F));
            buffer.Write((byte)((position & 0x00F0) >> 4));
            buffer.Write((byte)((position & 0x0F00) >> 8));
            buffer.Write((byte)((position & 0xF000) >> 12));

            buffer.Write((byte)(ID & 0x0F));
            buffer.Write((byte)((ID & 0xF0) >> 4));

            return base.Send(buffer, out send, out free);

            // TODO: add protocol

            return base.Send(buffer, out send, out free);
        }

        public static Command Parse(Controller c, string line)
        {
            string[] split = line.Split(' ');
            return new Command_MoveServo(c, Convert.ToInt32(split[1]), Convert.ToInt32(split[2]));
        }
    }
}
