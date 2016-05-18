using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Expression_Negation : Expression
    {
        Expression expr;

        public Expression_Negation(Expression expr)
        {
            this.expr = expr;
        }

        public override int Value()
        {
            return Convert.ToInt32(!Convert.ToBoolean(expr.Value()));
        }
    }
}
