using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace PuppetMaster
{
    public class Command_MoveTo : Command
    {
        float x, y;
        Expression exprX, exprY;

        float angle, distance;

        public Command_MoveTo(Controller c, float x, float y) //degrees
            : base(c)
        {
            this.exprX = null;
            this.exprY = null;
            this.x = x;
            this.y = y;

            Prepare();
        }

        public Command_MoveTo(Controller c, Expression exprX, Expression exprY)
            : base(c)
        {
            this.exprX = exprX;
            this.exprY = exprY;
            this.x = 0;
            this.x = 0;

            Prepare();
        }

        void Prepare()
        {
            float valX = (exprX == null) ? x : exprX.Value();
            float valY = (exprY == null) ? y : exprY.Value();

            float val = (float)Math.Atan2(valY - c.r.y, valX - c.r.x);

            float dir1 = (val - c.r.rot + 360) % 360;
            float dir2 = (c.r.rot - (val - 360)) % 360;

            this.angle = (Math.Abs(dir1) < Math.Abs(dir2)) ? dir1 : dir2;

            this.distance = (float)Math.Sqrt(Math.Pow(valX - c.r.x, 2) + Math.Pow(valY - c.r.y, 2));
        }

        bool rotationSent = false;

        public override bool Send(CommBuffer buffer, out bool send, out bool free)
        {
            Command comm = null;

            if (!rotationSent)
            {
                comm = new Command_Rotate(c, angle);

                free = false;
            }
            else
            {
                comm = new Command_Move(c, distance);

                free = true;
            }

            bool dummySend, dummyFree;

            comm.Send(buffer, out dummySend, out dummyFree);

            send = true;
            return true;
        }

        public static Command Parse(Controller c, string line)
        {
            string[] split = line.Split(' ');

            return new Command_MoveTo(c, Expression.Parse(split[2]), Expression.Parse(split[2]));

            //return new Command_MoveTo(c, Convert.ToInt32(split[1]), Convert.ToInt32(split[1]));
        }
    }
}
