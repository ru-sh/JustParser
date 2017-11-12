using System;
using System.Collections.Generic;
using System.Linq;

namespace JustParser
{
    public class Parser : IParser
    {
        private readonly string _debugStr = nameof(Parser);

        private readonly OrderedDictionary<string, IParser> _subParsers = null;

        public Parser(OrderedDictionary<string, IParser> parsers, string debugStr = null)
        {
            _debugStr = debugStr ?? "[" + string.Join(",", parsers.Select(pair => pair.Key)) + "]";
            _subParsers = parsers;
        }

        public IParseReader CreateParseReader()
        {
            return new ParserReader(_subParsers);
        }

        public IEnumerable<(string propName, IParser parser)> MakeFlat(
            OrderedDictionary<string, (string propName, IParser parser)> parsers)
        {
            foreach (var p in parsers)
            {
                var parserName = p.Key;
                var value = p.Value;
                var iparser = value.parser;
                if (iparser is Parser parser && parser._subParsers != null)
                {
                    var subs = parser._subParsers
                        .Select(pair => (parserName + '.' + pair.Key, pair.Value))
                        .ToList();

                    foreach (var val in subs)
                    {
                        yield return val;
                    }

                    yield break;
                }
            }
        }

        public override string ToString()
        {
            return _debugStr;
        }

        private class ParserReader : IParseReader
        {
            private readonly List<List<PropParser>> _currentParsers;
            private List<char> _debugBuf = new List<char>();

            public ParserReader(OrderedDictionary<string, IParser> parsers)
            {
                var propParsers = parsers.Select(pair => new PropParser(pair.Key, pair.Value, pair.Value.CreateParseReader())).ToList();
                _currentParsers = new List<List<PropParser>> { propParsers };
            }

            private ParserReader(List<List<PropParser>> parsers)
            {
                _currentParsers = parsers;
            }

            public ParserStatus Read(char c)
            {
                _debugBuf.Add(c);

                var propParsers = _currentParsers.ToArray();
                foreach (var branch in propParsers)
                {
                    var propParser = branch.FirstOrDefault(pair => pair.Value == null);
                    var last = branch.Last();
                    if (propParser == null || propParser == last)
                    {
                        propParser = last;
                        var status = propParser.Reader.Read(c);
                        propParser.SetValue(status.ExactMatch ? status.ParseredObject : null);
                    }
                    else
                    {
                        var parser = propParser.Reader;
                        var state = parser.Read(c);

                        if (!state.PartialMatch)
                        {
                            _currentParsers.Remove(branch);
                            PrintParserState(_debugBuf.AsString() + " " + parser.ToString());
                        }


                        if (state.ExactMatch)
                        {
                            var cpp = propParser.Clone();
                            cpp.SetValue(state.ParseredObject);
                            var copy = branch
                                .Select(selector: pp =>
                                {
                                    var clone = pp.Clone();
                                    if (clone.Key == propParser.Key)
                                    {
                                        clone.SetValue(state.ParseredObject);
                                    }

                                    return clone;
                                })
                                .ToList();

                            _currentParsers.Add(copy);

                            PrintParserState(_debugBuf.AsString() + " " + parser.ToString());
                        }
                    }
                }

                PrintParserState(_debugBuf.AsString());

                if (!_currentParsers.Any())
                {
                    return ParserStatus.Mismatch();
                }

                var exactMatch = _currentParsers.FirstOrDefault(dictionary => dictionary.All(tuple => tuple.Value != null));
                if (exactMatch != null)
                {
                    var dictionary = MatchToDict(exactMatch);
                    return ParserStatus.Exact(dictionary);
                }

                return ParserStatus.Partial();
            }


            private static object MatchToDict(ICollection<PropParser> match)
            {
                if (match.Any(property => property.Value == null)) return null;

                var result = new Dictionary<string, object>();
                foreach (var pair in match.Where(parser => !parser.Key.StartsWith('$')))
                {
                    var value = pair.Value;
                    if (value is ArraySegment<char>)
                        value = ((ArraySegment<char>)value).AsString();

                    result.Add(pair.Key, value);
                }

                return result;
            }

            [System.Diagnostics.Conditional("TRACE")]
            private void PrintParserState(string str)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine(str);
                foreach (var dictionary in _currentParsers)
                {
                    foreach (var kv in dictionary)
                    {
                        var st = kv.Value == null ? '-' : '+';
                        Console.WriteLine($"{st}{kv.Key.PadRight(20)}: {kv.Parser} [{kv.Reader}]");
                    }

                    Console.WriteLine("-----");
                }

                //            Thread.Sleep(20);
            }


            public IParseReader Clone()
            {
                var subParser = _currentParsers.Select(list => list.Select(propParser => propParser.Clone()).ToList()).ToList();
                var parser = new ParserReader(subParser)
                {
                    _debugBuf = _debugBuf.ToList()
                };
                return parser;
            }

            private class PropParser
            {
                public PropParser(string key, IParser parser, IParseReader reader)
                {
                    Key = key;
                    Parser = parser;
                    Reader = reader;
                }

                public string Key { get; }
                public IParser Parser { get; }
                public IParseReader Reader { get; }
                public object Value { get; private set; }

                public PropParser SetValue(object value)
                {
                    Value = value;
                    return this;
                }

                public PropParser Clone()
                {
                    return new PropParser(Key, Parser, Reader.Clone()) { Value = Value };
                }
            }

            public override string ToString()
            {
                return _debugBuf.AsString();
            }
        }
    }
}