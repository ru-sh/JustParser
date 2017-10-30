using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using CheapFlights.Importer.Lexing;

namespace JustParser
{
    public class Parser : IParser
    {
        private readonly string _debugStr = nameof(Parser);

        private readonly Func<ArraySegment<char>, object> _lexFunc;

        private readonly OrderedDictionary<string, IParser> _subLexers = new OrderedDictionary<string, IParser>();

        private static bool _debug = false;

        public Parser(Func<ArraySegment<char>, object> lexFunc)
        {
            _lexFunc = lexFunc;
        }

        public Parser(string rule)
        {
            var lexerProvider = ParseRule(rule, _subLexers);
            _debugStr = rule;

            _lexFunc = str =>
            {
                var lexers = lexerProvider.Select(pair => new KeyValuePair<string, IParser>(pair.Key, pair.Value())).ToList();
                var match = TryMatch(str, lexers).ToList();
                return MatchToDict(match);
            };
        }

        private static object MatchToDict(ICollection<KeyValuePair<string, object>?> match)
        {
            if (match.Any(property => property == null)) return null;

            var result = new Dictionary<string, object>();
            foreach (var pair in match.Select(pair => pair.Value))
            {
                var value = pair.Value;
                if (value is ArraySegment<char>) value = ((ArraySegment<char>) value).AsString();

                if (!pair.Key.StartsWith("$") && !result.TryAdd(pair.Key, value))
                {
                    throw new Exception("Duplicated keys");
                }
            }

            return result;
        }

        private static IEnumerable<KeyValuePair<string, Func<IParser>>> ParseRule(string rule, IDictionary<string, IParser> subLexers)
        {
            if (rule.Length == 0)
                yield break;

            var openIdx = rule.IndexOf('{');
            var closeIdx = rule.IndexOf('}');

            if (closeIdx < openIdx)
            {
                throw new InvalidDataException($"Unable to find open bracket: '{rule}'");
            }

            if (openIdx < 0)
            {
                yield return KeyValuePair.Create<string, Func<IParser>>("$" + Guid.NewGuid(), () => new ConstParser(rule));
            }
            else if (openIdx > 0)
            {
                var word = rule.Substring(0, openIdx);
                closeIdx = openIdx - 1;
                yield return KeyValuePair.Create<string, Func<IParser>>("$" + Guid.NewGuid(), () => new ConstParser(word));
            }
            else
            {
                openIdx++; // skip '{'
                if (closeIdx < 0)
                {
                    throw new InvalidDataException($"Unable to find close bracket: '{rule}'");
                }

                if (closeIdx < 2)
                {
                    throw new InvalidDataException($"Invalid parser name: '{rule}'");
                }

                var lexerDef = rule.Substring(openIdx, closeIdx - openIdx);
                var parts = lexerDef.Split(':', 2);
                var propName = parts[0];
                var lexerName = parts.Length > 1 ? parts[1] : propName;
                yield return KeyValuePair.Create<string, Func<IParser>>(propName, () => subLexers[lexerName]);
            }

            if (closeIdx < rule.Length)
            {
                var restRule = rule.Substring(closeIdx + 1);
                var subScan = ParseRule(restRule, subLexers);
                foreach (var pair in subScan)
                {
                    yield return pair;
                }
            }
        }

        public Parser(OrderedDictionary<string, IParser> lexers)
        {
            _lexFunc = s =>
            {
                var match = TryMatch(s, lexers.ToList()).ToList();
                return MatchToDict(match);
            };
        }

        private static IEnumerable<KeyValuePair<string, object>?> TryMatch(ArraySegment<char> str, ICollection<KeyValuePair<string, IParser>> lexers)
        {
            if (!lexers.Any())
            {
                yield break;
            }

            var first = lexers.First();
            var firstLexer = first.Value;

            if (lexers.Count == 1)
            {
                // last parser must match the whole substr
                var parsed = firstLexer.Parse(str);
                if (_debug)
                {
                    var success = parsed == null ? '-' : '+';
                    Console.WriteLine($"{success}{firstLexer} {str.AsString()}");
                }

                if (parsed == null)
                    yield return null;
                else
                    yield return new KeyValuePair<string, object>(first.Key, parsed);

                yield break;
            }

            int stop = -1;
            while (stop < str.Count - 1)
            {
                stop++;

                var firstStr = new ArraySegment<char>(str.Array, str.Offset, stop);
                var parsed = firstLexer.Parse(firstStr);
                if (_debug)
                {
                    var success = parsed == null ? '-' : '+';
                    Console.WriteLine($"{success}{firstLexer} {firstStr.AsString()}");
                }

                if (parsed == null)
                {
                    continue;
                }

                var rest = str.Slice(stop);

                var restLexers = lexers.Skip(1).ToList();
                var subScan = TryMatch(rest, restLexers).ToList();
                if (subScan.All(pair => pair != null))
                {
                    yield return new KeyValuePair<string, object>(first.Key, parsed);
                    foreach (var val in subScan)
                    {
                        yield return val;
                    }
                    yield break;
                }
            }

            yield return null;
        }

        public object Parse(string str)
        {
            return Parse(new ArraySegment<char>(str.ToCharArray()));
        }

        public Parser Where(string name, IParser parser)
        {
            this._subLexers.Add(name, parser);
            return this;
        }

        public Parser Where(string name, Func<ArraySegment<char>, IEnumerable<object>> subLexers)
        {
            var lexer = new Parser(s =>
            {
                var results = subLexers(s).ToArray();
                if (results.All(r => r != null))
                {
                    return results;
                }

                return null;
            });

            return this.Where(name, lexer);
        }

        public Parser Where(string name, Expression<Func<ArraySegment<char>, bool>> predicate)
        {
            var lexer = new ExpressionParser(predicate);
            return this.Where(name, lexer);
        }

        public Parser Where(string name, Predicate<ArraySegment<char>> predicate, Func<ArraySegment<char>, object> extractFunc)
        {
            var lexer = new Parser(s => predicate(s) ? extractFunc(s) : null);
            return this.Where(name, lexer);
        }

        //        public Parser Where(string name, Regex regex)
        //        {
        //            this._subLexers.Add(name, new Parser(s => regex.Match(s).Success ? s : null));
        //            return this;
        //        }

        public Parser WhereIsInt(string name)
        {
            return Where(name, new IsDigitsParser());
        }

        public override string ToString()
        {
            return _debugStr;
        }

        public object Parse(ArraySegment<char> chars)
        {
            return _lexFunc(chars);
        }
    }
}