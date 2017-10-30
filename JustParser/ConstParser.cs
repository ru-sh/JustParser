using System;

namespace JustParser
{
    public class ConstParser : IParser
    {
        private readonly string _constStr;
        private readonly char[] _constChars;

        public ConstParser(string constStr)
        {
            _constStr = constStr;
            _constChars = constStr.ToCharArray();
        }

        public object Parse(ArraySegment<char> str)
        {
            var isEqualsTo = _constChars.IsEqualsTo(str);
            if (isEqualsTo)
            {
                var asString = str.AsString();
                return asString;
            }

            return null;
        }

        public override string ToString()
        {
            return "'" + _constStr + "'";
        }
    }
}