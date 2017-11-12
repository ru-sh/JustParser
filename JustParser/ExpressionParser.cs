using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JustParser
{
    public class ExpressionParser : IParser
    {
        private readonly Expression<Func<IReadOnlyList<char>, bool>> _expr;
        private readonly bool _continueOnFail;
        private readonly Func<IReadOnlyList<char>, object> _parseFunc;
        private readonly Func<IReadOnlyList<char>, bool> _predicate;

        public ExpressionParser(Expression<Func<IReadOnlyList<char>, bool>> expr, Func<IReadOnlyList<char>, object> parseFunc = null, bool continueOnFail = true)
        {
            _expr = expr;
            _continueOnFail = continueOnFail;
            _parseFunc = parseFunc ?? (c => c.AsString());
            _predicate = _expr.Compile();
        }

        public override string ToString()
        {
            return _expr.ToString();
        }

        public IParseReader CreateParseReader()
        {
            return new Reader(_predicate, _parseFunc, _continueOnFail);
        }

        private class Reader : IParseReader
        {
            List<char> _buf = new List<char>();
            private readonly Func<IReadOnlyList<char>, bool> _predicate;
            private readonly Func<IReadOnlyList<char>, object> _parseFunc;
            private readonly bool _continueOnFail;

            public Reader(Func<IReadOnlyList<char>, bool> predicate, Func<IReadOnlyList<char>, object> parseFunc, bool continueOnFail)
            {
                _predicate = predicate;
                _parseFunc = parseFunc;
                _continueOnFail = continueOnFail;
            }

            public ParserStatus Read(char c)
            {
                _buf.Add(c);
                if (_predicate(_buf)) return ParserStatus.Exact(_parseFunc(_buf));
                return _continueOnFail ? ParserStatus.Partial() : ParserStatus.Mismatch();
            }

            public IParseReader Clone()
            {
                var parser = new Reader(_predicate, _parseFunc, _continueOnFail) { _buf = _buf.ToList() };
                return parser;
            }

            public override string ToString()
            {
                return _buf.AsString();
            }
        }
    }
}