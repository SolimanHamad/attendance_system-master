using Newtonsoft.Json.Converters;

namespace AttendanceSystem.Data.JsonParsers
{
    public class HumanDateConverter : IsoDateTimeConverter
    {
        public HumanDateConverter()
        {
            DateTimeFormat = "yyyy/MM/dd h:mm tt";
        }
    }
}