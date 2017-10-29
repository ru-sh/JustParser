using System;
using System.Linq.Expressions;

namespace JustParser
{
    public class ExpressionParser : IParser
    {
        private readonly Expression<Func<string, bool>> _expr;
        private readonly Func<string, bool> _predicate;

        public ExpressionParser(Expression<Func<string, bool>> expr)
        {
            _expr = expr;
            _predicate = _expr.Compile();
        }

        public object Parse(string str)
        {
            if (_predicate(str))
            {
                return str;
            }

            return null;
        }

        public override string ToString()
        {
            return _expr.ToString();
        }
    }
}