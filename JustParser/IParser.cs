using System.Collections.Generic;

namespace JustParser
{
    public interface IParser
    {
        IParseReader CreateParseReader();
    }

    public interface IParseReader
    {
        ParserStatus Read(char c);
        IParseReader Clone();
    }
}