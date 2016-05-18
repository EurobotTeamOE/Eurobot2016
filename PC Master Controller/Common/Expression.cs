using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public abstract class Expression
    {
        private static Dictionary<string, Expression> expressions = new Dictionary<string, Expression>();

        public abstract int Value();

        public static void Define(Robot r, string line)
        {
            string[] split = line.Split(' ');

            Expression expr = Parse(split[2], r);
            expressions.Add(split[1], expr);
        }

        public static Expression Parse(string expr, Robot r = null)
        {
            Expression expression;
            bool negate = false;

            while (expr.StartsWith("!"))
            {
                negate = !negate;
                expr = expr.Substring(1);
            }

            if (expressions.ContainsKey(expr)) expression = expressions[expr];
            else
            {
                bool constant = !expr.StartsWith("{");

                if (!constant) expr = expr.Substring(1, expr.Length - 2);
                int num = Convert.ToInt32(expr);

                if (constant) expression = new Expression_Constant(num);
                else expression = new Expression_StateBit(r, num);
            }

            return negate ? new Expression_Negation(expression) : expression; 
        }
    }
}
