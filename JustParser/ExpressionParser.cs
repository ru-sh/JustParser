using System;
using System.Linq.Expressions;

namespace JustParser
{
    public class ExpressionParser : IParser
    {
        private readonly Expression<Func<ArraySegment<char>, bool>> _expr;
        private readonly Func<ArraySegment<char>, bool> _predicate;

        public ExpressionParser(Expression<Func<ArraySegment<char>, bool>> expr)
        {
            _expr = expr;
            _predicate = _expr.Compile();
        }

        public object Parse(ArraySegment<char> str)
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