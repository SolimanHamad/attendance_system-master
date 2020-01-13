using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace AttendanceSystem.TagHelpers
{
    public class WidgetGridTagHelper : TagHelper
    {

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "section";
            output.Attributes.SetAttribute("id", "widget-grid");
            output.Content.AppendHtml(await output.GetChildContentAsync());
        }
    }
}
