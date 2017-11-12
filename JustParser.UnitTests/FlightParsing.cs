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
            var hourLabels = new[] { "���", "����", "�����" };
            var placeParser = new ParserBuilder(" {city} {hours} {hoursLabel}")
                    .Where("city", s => !s.Contains(','), continueOnFail: false)
                    .WhereIsDigits("hours")
                    .Where("hoursLabel", s => s.IsOneOf(hourLabels))
                    .Build()
                ;

            //            return placeParser.Parse(" Abu-Dabi 13 �����");

            var placesParser = new RepeatableParser(placeParser, new ConstParser(","));

            //            return placesParser.Parse(" ������ 17 �����, ���-���� 13 �����");

            var companyParser = new ExpressionParser(s => !s.Contains(',') && !s.Contains("���"), c => c.AsString());

            var connectionsParser = new ParserBuilder("{places}, {companies}")
                .Where("places", placesParser)
                .Where("companies", new RepeatableParser(companyParser, new ConstParser(", ")))
                .Build();

            var j = JsonConvert.SerializeObject(
                connectionsParser.Parse(" ������ 17 �����, ���-���� 13 �����, �������, Etihad Airways"),
                Formatting.Indented);

            var directionParser = new ParserBuilder("{count} {countLabel} �{connections}")
                .WhereIsDigits("count")
                .Where("countLabel", s => s.IsOneOf(new[] { "���������", "���������", "���������" }))
                .Where("connections", connectionsParser)
                .Build();

            //return directionParser.Parse("2 ��������� � ������ 17 �����, ���-���� 13 �����, �������, Etihad Airways");

            var footer = @"���� 2 ��������� � ������ 17 �����, ���-���� 13 �����, �������, Etihad Airways\n�������: 1 ��������� � �������� 6 �����, Pegasus Airlines";

            var parser = new ParserBuilder(@"���� {src:direction}\n�������: {dst:direction}")
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
    ""countLabel"": ""���������"",
    ""connections"": {
      ""places"": [
        {
          ""city"": ""������"",
          ""hours"": 17,
          ""hoursLabel"": ""�����""
        }
      ],
      ""companies"": [
        ""���-���� 13 �����"",
        ""�������"",
        ""Etihad Airways""
      ]
    }
  },
  ""dst"": {
    ""count"": 1,
    ""countLabel"": ""���������"",
    ""connections"": {
      ""places"": [
        {
          ""city"": ""��������"",
          ""hours"": 6,
          ""hoursLabel"": ""�����""
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