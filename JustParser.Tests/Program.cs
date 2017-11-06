using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace JustParser.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1; i++)
            {
                var result = Test2();

                Console.WriteLine(sw.Elapsed.TotalMilliseconds + "ms");
                Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            }

            Console.ReadLine();
        }

        private static object TestEmailOnExpression()
        {
            var str = "test@a.sd";

            var emailParser = new Parser(new OrderedDictionary<string, IParser>()
                {
                    {"name", new ExpressionParser(s => !s.Contains('@')) },
                    {"@", new ConstParser("@") },
                    {"domain", new ExpressionParser(s => !s.Contains('@')) },
                })
                ;

            var jToken = emailParser.Parse(str);
            return jToken;
        }


        private static object TestEmail()
        {
            var str = "test@a.sd";

            var emailParser = new ParserBuilder("{name}@{domain}")
                    .Where("name", s => !s.Contains('@')) // .Where(name=> !name.Contains...)
                    .Where("domain", s => !s.Contains('@') && !s.Contains('/'))
                    .Build()
                ;

            var jToken = emailParser.Parse(str);
            return jToken;
        }

        static object Test2()
        {
            var placeParser = new ParserBuilder(" {city} {hours} {hoursLabel}")
                    .Where("city", s => !s.Contains(','), continueOnFail: false)
                    .WhereIsDigits("hours")
                    .Where("hoursLabel", s => s.IsOneOf(new[] { "час", "часа", "часов" }))
                    .Build()
                ;

            //            return placeParser.Parse(" Abu-Dabi 13 часов");

            var placesParser = new RepeatableParser(placeParser, new ConstParser(","));

            //            return placesParser.Parse(" Москве 17 часов, Абу-Даби 13 часов");

            var companyParser = new ExpressionParser(s => !s.Contains(',') && placeParser.Parse(s.AsString()) == null, c => c.AsString());

            var connectionsParser = new ParserBuilder("{places}, {companies}")
                .Where("places", placesParser)
                .Where("companies", new RepeatableParser(companyParser, new ConstParser(", ")))
                .Build();

            //return connectionsParser.Parse(" Москве 17 часов, Абу-Даби 13 часов, Белавиа, Etihad Airways");

            var directionParser = new ParserBuilder("{count} {countLabel} в{connections}")
                .WhereIsDigits("count")
                .Where("countLabel", s => s.IsOneOf(new[] { "пересадка", "пересадки", "пересадок" }))
                .Where("connections", connectionsParser)
                .Build();

            //return directionParser.Parse("2 пересадки в Москве 17 часов, Абу-Даби 13 часов, Белавиа, Etihad Airways");

            var footer = @"Туда 2 пересадки в Москве 17 часов, Абу-Даби 13 часов, Белавиа, Etihad Airways\nОбратно: 1 пересадка в Стамбуле 6 часов, Pegasus Airlines";

            var lexer = new ParserBuilder(@"Туда {src:direction}\nОбратно: {dst:direction}")
                    .Where("direction", directionParser)
                    .Build()
                ;

            return lexer.Parse(footer);
        }
    }
}
