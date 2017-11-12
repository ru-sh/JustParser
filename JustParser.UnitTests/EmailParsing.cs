using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace JustParser.UnitTests
{
    public class EmailParsing
    {
        [Fact]
        public void ParseEmail()
        {
            var str = "test@a.sd";

            var emailParser = new Parser(new OrderedDictionary<string, IParser>()
                {
                    {"name", new ExpressionParser(s => !s.Contains('@')) },
                    {"@", new ConstParser("@") },
                    {"domain", new ExpressionParser(s => !s.Contains('@')) },
                })
                ;

            var obj = emailParser.Parse(str);
            Assert(obj);
        }

        [Fact]
        public void ParseEmailFromTemplate()
        {
            var str = "test@a.sd";

            var emailParser = new ParserBuilder("{name}@{domain}")
                    .Where("name", s => !s.Contains('@'), continueOnFail: false) // .Where(name=> !name.Contains...)
                    .Where("domain", s => !s.Contains('@') && !s.Contains('/'))
                    .Build()
                ;

            var obj = emailParser.Parse(str);
            Assert(obj);
        }

        private static void Assert(object obj)
        {
            Xunit.Assert.NotNull(obj);

            var dict = obj as Dictionary<string, object>;
            Xunit.Assert.NotNull(dict);

            Xunit.Assert.Equal("test", dict["name"]);
            Xunit.Assert.Equal("a.sd", dict["domain"]);
        }
    }
}
