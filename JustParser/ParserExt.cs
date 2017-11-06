namespace JustParser
{
    public static class ParserExt
    {
        public static object Parse(this IParser parser, string str)
        {
            var status = ParserStatus.Mismatch();
            var reader = parser.CreateParseReader();
            foreach (var c in str)
            {
                status = reader.Read(c);
                if (!status.PartialMatch)
                {
                    return null;
                }
            }

            if (status.ExactMatch)
            {
                return status.ParseredObject;
            }
            return null;
        }
    }
}