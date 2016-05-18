using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace PuppetMaster
{
    public class Command_LetGo : Command
    {
        Arm arm;

        public Command_LetGo(Controller c, Arm arm)
            : base(c)
        {
            this.arm = arm;
        }

        public override bool Send(CommBuffer buffer, out bool send, out bool free)
        {
            // Metadata
            buffer.Write(0xFF);
            buffer.Write(0x09);
            buffer.Write(5);

            // Data
            byte command = 0;
            if (arm == Arm.Left) command = 0x03;
            if (arm == Arm.Right) command = 0x07;
            if (arm == Arm.Cup) command = 0x0D;
            //byte command = (byte)((arm == Arm.Left) ? 0x03 : 0x07); //0x0D

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

            return new Command_LetGo(c, arm);
        }
    }
}
