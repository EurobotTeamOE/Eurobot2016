using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace PuppetMaster
{
    public class Command_Move : Command
    {
        float dist;
        Expression expr;

        public Command_Move(Controller c, float dist)
            : base(c)
        {
            this.expr = null;
            this.dist = dist;
        }

        public Command_Move(Controller c, Expression expr)
            : base(c)
        {
            this.expr = expr;
            this.dist = 0;
        }

        public override bool Send(CommBuffer buffer, out bool send, out bool free)
        {
            float val = (expr == null) ? dist : expr.Value();

            int pomeraj = (int)(val * 11.7374f);

            // Metadata
            buffer.Write(0xFF);
            buffer.Write(0x0A);
            buffer.Write(8);

            // Data
            buffer.Write((byte)(val >= 0 ? 0x04 : 0x05));
            //buffer.Write((byte)(val >= 0 ? 0xF1 : 0xF2));
            //buffer.Write((byte)0x00);

            pomeraj = Math.Abs(pomeraj);
            buffer.Write((byte)(pomeraj & 0x000F));
            buffer.Write((byte)((pomeraj & 0x00F0) >> 4));
            buffer.Write((byte)((pomeraj & 0x0F00) >> 8));
            buffer.Write((byte)((pomeraj & 0xF000) >> 12));

            buffer.Write((byte)(ID & 0x0F));
            buffer.Write((byte)((ID & 0xF0) >> 4));

            return base.Send(buffer, out send, out free);
        }

        public static Command Parse(Controller c, string line)
        {
            string[] split = line.Split(' ');

            return new Command_Move(c, Expression.Parse(split[1]));
            
            //return new Command_Move(c, Convert.ToInt32(split[1]));
        }
    }
}
