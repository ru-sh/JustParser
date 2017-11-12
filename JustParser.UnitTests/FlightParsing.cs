using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace JustParser.UnitTests
{
    public class FlightParsing
    {
        [Fact]
        public void ParseFlightInfo()
        {
            var hourLabels = new[] { "час", "часа", "часов" };
            var placeParser = new ParserBuilder(" {city} {hours} {hoursLabel}")
                    .Where("city", s => !s.Contains(','), continueOnFail: false)
                    .WhereIsDigits("hours")
                    .Where("hoursLabel", s => s.IsOneOf(hourLabels))
                    .Build()
                ;

            //            return placeParser.Parse(" Abu-Dabi 13 часов");

            var placesParser = new RepeatableParser(placeParser, new ConstParser(","));

            //            return placesParser.Parse(" Москве 17 часов, Абу-Даби 13 часов");

            var companyParser = new ExpressionParser(s => !s.Contains(',') && !s.Contains("час"), c => c.AsString());

            var connectionsParser = new ParserBuilder("{places}, {companies}")
                .Where("places", placesParser)
                .Where("companies", new RepeatableParser(companyParser, new ConstParser(", ")))
                .Build();

            var j = JsonConvert.SerializeObject(
                connectionsParser.Parse(" Москве 17 часов, Абу-Даби 13 часов, Белавиа, Etihad Airways"),
                Formatting.Indented);

            var directionParser = new ParserBuilder("{count} {countLabel} в{connections}")
                .WhereIsDigits("count")
                .Where("countLabel", s => s.IsOneOf(new[] { "пересадка", "пересадки", "пересадок" }))
                .Where("connections", connectionsParser)
                .Build();

            //return directionParser.Parse("2 пересадки в Москве 17 часов, Абу-Даби 13 часов, Белавиа, Etihad Airways");

            var footer = @"Туда 2 пересадки в Москве 17 часов, Абу-Даби 13 часов, Белавиа, Etihad Airways\nОбратно: 1 пересадка в Стамбуле 6 часов, Pegasus Airlines";

            var parser = new ParserBuilder(@"Туда {src:direction}\nОбратно: {dst:direction}")
                    .Where("direction", directionParser)
                    .Build()
                ;

            var result = parser.Parse(footer);
            AssertResult(result);
        }

        private void AssertResult(object result)
        {
            Assert.NotNull(result);
            var dict = result as Dictionary<string, object>;
            Assert.NotNull(dict);

            var json = JsonConvert.SerializeObject(dict, Formatting.Indented);
            Assert.Equal(@"{
  ""src"": {
    ""count"": 2,
    ""countLabel"": ""пересадки"",
    ""connections"": {
      ""places"": [
        {
          ""city"": ""Москве"",
          ""hours"": 17,
          ""hoursLabel"": ""часов""
        }
      ],
      ""companies"": [
        ""Абу-Даби 13 часов"",
        ""Белавиа"",
        ""Etihad Airways""
      ]
    }
  },
  ""dst"": {
    ""count"": 1,
    ""countLabel"": ""пересадка"",
    ""connections"": {
      ""places"": [
        {
          ""city"": ""Стамбуле"",
          ""hours"": 6,
          ""hoursLabel"": ""часов""
        }
      ],
      ""companies"": [
        ""Pegasus Airlines""
      ]
    }
  }
}", json);
        }
    }
}