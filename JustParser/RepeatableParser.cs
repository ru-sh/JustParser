using System;
using System.Collections.Generic;
using System.Linq;

namespace JustParser
{
    public class RepeatableParser : IParser
    {
        private readonly IParser _delimeterParser;
        private readonly IParser _itemParser;

        public RepeatableParser(IParser itemParser) : this(itemParser, null)
        {
        }

        public RepeatableParser(IParser itemParser, IParser delimeterParser)
        {
            _delimeterParser = delimeterParser;
            _itemParser = itemParser ?? throw new ArgumentNullException(nameof(itemParser));

            var empty = string.Empty;
            if (itemParser.Parse(empty) != null &&
                (delimeterParser == null || delimeterParser.Parse(empty) != null))
            {
                throw new Exception("Possibility of infinite loop.");
            }
        }

        public override string ToString()
        {
            return nameof(RepeatableParser);
        }

        private class ParserItem
        {
            public ParserItem(bool isItem, IParser parser, IParseReader reader)
            {
                IsItem = isItem;
                Parser = parser;
                Reader = reader;
            }

            public bool IsItem { get; }
            public IParser Parser { get; }
            public IParseReader Reader { get; }

            public object Value { get; private set; }

            public void SetValue(object value)
            {
                Value = value;
            }

            public ParserItem Clone()
            {
                return new ParserItem(IsItem, Parser, Reader.Clone())
                {
                    Value = Value
                };
            }
        }

        public IParseReader CreateParseReader()
        {
            return new Reader(_itemParser, _delimeterParser);
        }

        class Reader : IParseReader
        {
            private readonly IParser _delimeterParser;
            private readonly IParser _itemParser;
            private List<List<ParserItem>> _branches;
            
            public Reader(IParser itemParser, IParser delimeterParser)
            {
                _itemParser = itemParser;
                _delimeterParser = delimeterParser;
                
                _branches = new List<List<ParserItem>>()
                {
                    new List<ParserItem>()
                };
            }

            public ParserStatus Read(char c)
            {
                foreach (var branch in _branches.ToArray())
                {
                    var last = GetLast(branch);
                    var parserStatus = last.Reader.Read(c);

                    if (parserStatus.ExactMatch)
                    {
                        var newBranch = branch.Select(item => item.Clone()).ToList();
                        last = newBranch.Last();
                        last.SetValue(parserStatus.ParseredObject);
                        _branches.Add(newBranch);
                    }
                    else if (!parserStatus.PartialMatch)
                    {
                        _branches.Remove(branch);
                    }
                }

                if (!_branches.Any())
                {
                    return ParserStatus.Mismatch();
                }

                var exactMatch = _branches.FirstOrDefault(list => list.Any() && list.All(item => item.Value != null));
                if (exactMatch != null)
                {
                    var list = exactMatch
                        .Where(item => item.IsItem)
                        .Select(item => item.Value)
                        .ToList();
                    return ParserStatus.Exact(list);
                }

                return ParserStatus.Partial();
            }

            private ParserItem GetLast(ICollection<ParserItem> branch)
            {
                var last = branch.LastOrDefault();
                if (last == null || last.Value != null)
                {
                    var isItem = _delimeterParser == null || branch.Count % 2 == 0;
                    var parser = (isItem ? _itemParser : _delimeterParser);
                    var reader = parser.CreateParseReader();
                    last = new ParserItem(isItem, parser, reader);
                    branch.Add(last);
                }

                return last;
            }

            public IParseReader Clone()
            {
                return new Reader(_itemParser, _delimeterParser)
                {
                    _branches = _branches.Select(list => list.Select(item => item.Clone()).ToList()).ToList()
                };
            }

        }
    }
}