namespace JustParser
{
    public class ConstParser : IParser
    {
        private readonly string _constStr;

        public ConstParser(string constStr)
        {
            _constStr = constStr;
        }

        public object Parse(string str)
        {
            return _constStr == str ? str : null;
        }

        public override string ToString()
        {
            return _constStr;
        }
    }
}