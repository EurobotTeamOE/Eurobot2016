using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace PuppetMaster
{
    public class Command_Wait : Command
    {
        Expression expr;

        public Command_Wait(Controller c, Expression expr)
            : base(c)
        {
            this.expr = expr;
        }

        public override bool Send(CommBuffer buffer, out bool send, out bool free)
        {
            send = false;
            free = true;

            return Convert.ToBoolean(expr.Value());
        }

        public static Command Parse(Controller c, string line)
        {
            string[] split = line.Split(' ');

            Expression expr = Expression.Parse(split[1], c.r);

            return new Command_Wait(c, expr);
        }
    }
}
