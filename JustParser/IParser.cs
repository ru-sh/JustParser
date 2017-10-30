using System;

namespace JustParser
{
    public interface IParser
    {
        object Parse(ArraySegment<char> chars);
    }
}