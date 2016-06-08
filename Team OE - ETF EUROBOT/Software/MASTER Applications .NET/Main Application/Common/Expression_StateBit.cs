using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Expression_StateBit : Expression
    {
        Robot r;
        int index;

        public Expression_StateBit(Robot r, int index)
        {
            this.r = r;
            this.index = index;
        }

        public override int Value()
        {
            return r.GetStatusBit(index) ? 1 : 0;
        }
    }
}
