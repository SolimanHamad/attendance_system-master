using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AttendanceSystem.Extensions
{
    public static class TagHelpersExtensions
    {
        public static string DisplayOrName(this ModelMetadata metadata)
        {
            return metadata.DisplayName ?? metadata.Name.ToSentenceCase();
        }
    }
}
