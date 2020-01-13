using Newtonsoft.Json.Converters;

namespace AttendanceSystem.Data.JsonParsers
{
    public class OnlyDateConverter : IsoDateTimeConverter
    {
        public OnlyDateConverter()
        {
            DateTimeFormat = "yyyy-MM-dd";
        }
    }
}