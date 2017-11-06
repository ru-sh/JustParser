using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace JustParser
{
    public class ParserBuilder
    {
        private readonly OrderedDictionary<string, IParser> _subParsers = new OrderedDictionary<string, IParser>();
        private readonly string _debugStr = nameof(Parser);

        public ParserBuilder(string debugStr, OrderedDictionary<string, IParser> subParsers)
        {
            _debugStr = debugStr;
            _subParsers = subParsers;
        }

        public ParserBuilder(string rule)
        {
            var subParsers = ParseRule(new ArraySegment<char>(rule.ToCharArray()));
            foreach (var pair in subParsers)
            {
                _subParsers.Add(pair.Key, pair.Value);
            }
        }

        private static IEnumerable<KeyValuePair<string, IParser>> ParseRule(ArraySegment<char> rule)
        {
            if (rule.Count == 0)
                yield break;

            var openIdx = rule.IndexOf('{');
            var closeIdx = rule.IndexOf('}');

            if (closeIdx < openIdx)
            {
                throw new InvalidDataException($"Unable to find open bracket: '{rule}'");
            }

            if (openIdx != 0)
            {
                string str;
                if (openIdx < 0)
                {
                    str = rule.AsString();
                }
                else
                {
                    closeIdx = openIdx - 1;
                    var word = rule.Slice(0, openIdx);
                    str = word.AsString();
                }

                yield return KeyValuePair.Create<string, IParser>($"${rule.Offset} '{str}'",
                    new ConstParser(str));
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

                var lexerDef = rule.Slice(openIdx, closeIdx - openIdx).AsString();
                yield return KeyValuePair.Create<string, IParser>(lexerDef, null);
            }

            if (closeIdx >= rule.Count) yield break;

            var restRule = rule.Slice(closeIdx + 1);
            var subScan = ParseRule(restRule);
            foreach (var pair in subScan)
            {
                yield return pair;
            }
        }

        public ParserBuilder Where(string name, IParser parser)
        {
            var found = _subParsers.Where(pair =>
            {
                var parts = pair.Key.Split(':', 2);
                var propName = parts[0];
                var lexerName = parts.Length > 1 ? parts[1] : propName;
                return lexerName == name;
            }).ToList();

            if (!found.Any())
            {
                throw new Exception($"Parser with name '{name}' is not found.");
            }

            foreach (var pair in found)
            {
                _subParsers[pair.Key] = parser;
            }

            return this;
        }

        public ParserBuilder Where(
            string name,
            Expression<Func<IReadOnlyCollection<char>, bool>> predicate,
            Func<IReadOnlyCollection<char>, object> parseFunc = null,
            bool continueOnFail = true)
        {
            parseFunc = parseFunc ?? (c => c.AsString());
            var lexer = new ExpressionParser(predicate, parseFunc, continueOnFail);
            return this.Where(name, lexer);
        }

        public ParserBuilder WhereIsDigits(string name)
        {
            return Where(name, new ExpressionParser(cs => cs.All(char.IsDigit), cs => int.Parse(cs.AsString()), false));
        }

        public Parser Build()
        {
            foreach (var parser in _subParsers)
            {
                if (parser.Value == null)
                {
                    throw new Exception($"Unable to find '{parser.Key}' parser.");
                }
            }

            var parsers = _subParsers.ToOrderedDictionary(pair =>
            {
                var key = pair.Key;
                var i = key.IndexOf(':');
                if (i > 0)
                {
                    key = key.Substring(0, i);
                }
                return key;
            }, pair => pair.Value);
            return new Parser(parsers, _debugStr);
        }
    }
}