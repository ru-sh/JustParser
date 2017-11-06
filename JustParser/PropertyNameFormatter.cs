namespace JustParser
{
    public class PropertyNameFormatter
    {
        public string Format(string propName, string lexerName)
        {
            return $"{propName}:{lexerName}";
        }

        public (string propName, string lexerName) Parse(string key)
        {
            var parts = key.Split(':', 2);
            var propName = parts[0];
            var lexerName = parts.Length > 1 ? parts[1] : propName;
            return (propName, lexerName);
        }
    }
}