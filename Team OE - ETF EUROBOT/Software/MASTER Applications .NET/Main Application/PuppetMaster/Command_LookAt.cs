using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace PuppetMaster
{
    public class Command_LookAt : Command
    {
        float x, y;
        Expression exprX, exprY;

        public Command_LookAt(Controller c, float x, float y) //degrees
            : base(c)
        {
            this.exprX = null;
            this.exprY = null;
            this.x = x;
            this.y = y;
        }

        public Command_LookAt(Controller c, Expression exprX, Expression exprY)
            : base(c)
        {
            this.exprX = exprX;
            this.exprY = exprY;
            this.x = 0;
            this.x = 0;
        }

        public override bool Send(CommBuffer buffer, out bool send, out bool free)
        {
            float valX = (exprX == null) ? x : exprX.Value();
            float valY = (exprY == null) ? y : exprY.Value();

            float val = (float)Math.Atan2(valY - c.r.y, valX - c.r.x);

            float dir1 = (val - c.r.rot + 360) % 360;
            float dir2 = (c.r.rot - (val - 360)) % 360;

            val = (Math.Abs(dir1) < Math.Abs(dir2)) ? dir1 : dir2;

            // Metadata
            buffer.Write(0xFF);
            buffer.Write(0x0A);
            buffer.Write((byte)(8));

            // Data
            buffer.Write((byte)(val > 0 ? 0x06 : 0x07));
            //buffer.Write((byte)0x00);

            int Ugao = (int)(Math.Abs(val) * 3100 / 90);
            buffer.Write((byte)(Ugao & 0x000F));
            buffer.Write((byte)((Ugao & 0x00F0) >> 4));
            buffer.Write((byte)((Ugao & 0x0F00) >> 8));
            buffer.Write((byte)((Ugao & 0xF000) >> 12));

            buffer.Write((byte)(ID & 0x0F));
            buffer.Write((byte)((ID & 0xF0) >> 4));

            return base.Send(buffer, out send, out free);
        }

        public static Command Parse(Controller c, string line)
        {
            string[] split = line.Split(' ');

            return new Command_LookAt(c, Expression.Parse(split[2]), Expression.Parse(split[2]));

            //return new Command_LookAt(c, Convert.ToInt32(split[1]), Convert.ToInt32(split[1]));
        }
    }
}
