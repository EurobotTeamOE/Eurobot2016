using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace PuppetMaster
{
    public class Command_PickUp : Command
    {
        Arm arm;
        bool last;

        public Command_PickUp(Controller c, Arm arm, bool last)
            : base(c)
        {
            this.arm = arm;
            this.last = last;
        }

        public override bool Send(CommBuffer buffer, out bool send, out bool free)
        {
            // Metadata
            buffer.Write(0xFF);
            buffer.Write(0x09);
            buffer.Write(5);

            // Data

            byte command = 0;
            if (arm == Arm.Left) command = 0x01;
            if (arm == Arm.Right) command = 0x05;
            if (arm == Arm.Cup) command = 0x0C;

            //byte command = (byte)((arm == Arm.Left) ? 0x01 : 0x05); //0x0C
            if (last) command++;
            
            buffer.Write(command);
            buffer.Write(0x00);
            buffer.Write((byte)(ID & 0x0F));
            buffer.Write((byte)((ID & 0xF0) >> 4));

            return base.Send(buffer, out send, out free);
        }

        public static Command Parse(Controller c, string line)
        {
            string[] split = line.Split(' ');

            Arm arm = 0;

            string upper = split[1].ToUpper();
            if (upper.StartsWith("L")) arm = Arm.Left;
            if (upper.StartsWith("R")) arm = Arm.Right;
            if (upper.StartsWith("C")) arm = Arm.Cup;

            return new Command_PickUp(c, arm, split[1].EndsWith("!"));
        }
    }
}
