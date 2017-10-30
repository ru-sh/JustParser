using System;

namespace JustParser
{
    public class IsDigitsParser : IParser
    {
        public object Parse(ArraySegment<char> str)
        {
            if (str.IsDigits())
            {
                var s = str.AsString();
                var asString = int.Parse(s);
                return asString;
            }

            return null;
        }

        public override string ToString()
        {
            return "IsDigits";
        }
    }
}