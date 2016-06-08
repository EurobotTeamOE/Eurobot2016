using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Expression_Constant : Expression
    {
        int value;

        public Expression_Constant(int value)
        {
            this.value = value;
        }

        public override int Value()
        {
            return value;
        }
    }
}
