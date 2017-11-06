using System;
using System.Collections.Generic;
using System.Linq;

namespace JustParser
{
    public class ConstParser : IParser
    {
        private readonly string _constStr;
        
        public ConstParser(string constStr)
        {
            _constStr = constStr;
        }

        public override string ToString()
        {
            return "'" + _constStr + "'";
        }

        public IParseReader CreateParseReader()
        {
            return new Reader(_constStr);
        }

        private class Reader : IParseReader
        {
            private readonly string _constStr;
            private readonly ArraySegment<char> _constChars;

            private int _i = 0;
            private bool _failed = false;
            private List<char> _buf = new List<char>();

            public Reader(string constStr)
            {
                _constStr = constStr;
                _constChars = constStr.ToCharArray();
            }

            public ParserStatus Read(char c)
            {
                _buf.Add(c);
                if (_i >= _constChars.Count || _constChars[_i] != c || _failed)
                {
                    _failed = true;
                    return (ParserStatus.Mismatch());
                }

                if (_i + 1 == _constChars.Count)
                    return (ParserStatus.Exact(_constStr));

                _i++;
                return (ParserStatus.Partial());
            }

            public IParseReader Clone()
            {
                return new Reader(_constStr)
                {
                    _failed = _failed,
                    _i = _i,
                    _buf = _buf.ToList()
                };
            }

            public override string ToString()
            {
                return _buf.AsString();
            }
        }
    }
}