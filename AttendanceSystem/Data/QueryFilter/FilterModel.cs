namespace AttendanceSystem.Data.QueryFilter
{
    public class FilterModel
    {
        public string Property { get; set; }
        public ComparisonType ComparisonType { get; set; }
        public string Value { get; set; }
        public string Value2 { get; set; } // Used for range filters
    }
}
