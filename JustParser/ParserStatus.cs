using System.Diagnostics;

namespace JustParser
{
    public struct ParserStatus
    {
        [DebuggerStepThrough]
        public static ParserStatus Exact(object val)
        {
            return new ParserStatus(val);
        }

        [DebuggerStepThrough]
        public static ParserStatus Partial()
        {
            return new ParserStatus(true, false, null);
        }

        [DebuggerStepThrough]
        public static ParserStatus Mismatch()
        {
            return new ParserStatus(null);
        }
        
        public ParserStatus(object v)
        {
            this.ExactMatch = this.PartialMatch = v != null;
            this.ParseredObject = v;
        }

        public ParserStatus(bool partialMatch, bool exactMatch, object parseredObject)
        {
            PartialMatch = partialMatch;
            ExactMatch = exactMatch;
            ParseredObject = parseredObject;
        }

        public bool PartialMatch { get;  }
        public bool ExactMatch { get; }
        public object ParseredObject { get; }
    }
}