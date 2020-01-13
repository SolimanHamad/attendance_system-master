using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AttendanceSystem.TagHelpers
{
    [HtmlTargetElement("widget",Attributes = "widget-id")]
    public class WidgetTagHelper : TagHelper
    {
        [HtmlAttributeName("widget-id")]
        public string ID { get; set; }

        [HtmlAttributeName("widget-name")]
        public string Name { get; set; }

        [HtmlAttributeName("widget-icon")]
        public string Icon { get; set; }

        [HtmlAttributeName("colorable")]
        public bool Colorable { get; set; } = true;

        [HtmlAttributeName("editable")]
        public bool Editable { get; set; } = true;

        [HtmlAttributeName("collapsible")]
        public bool Collapsible { get; set; } = true;

        [HtmlAttributeName("sortable")]
        public bool Sortable { get; set; } = true;

        [HtmlAttributeName("fullscreen-button")]
        public bool FullScreenButton { get; set; } = true;

        [HtmlAttributeName("body-padding")]
        public bool BodyPaddings { get; set; } = true;

        // Not exposed as property yet, not sure if we need it ever!
        public bool Deletable { get; set; } = false;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("id", ID);
            output.AddClass("jarviswidget", HtmlEncoder.Default);
            output.AddClass("jarviswidget-color-darken", HtmlEncoder.Default);

            // Configure the widget
            if (!Sortable)
                output.Attributes.Add("data-widget-sortable", false);
            if (!Colorable)
                output.Attributes.Add("data-widget-colorbutton", false);
            if (!Editable)
                output.Attributes.Add("data-widget-editbutton", false);
            if (!Collapsible)
                output.Attributes.Add("data-widget-togglebutton", false);
            if (!Deletable)
                output.Attributes.Add("data-widget-deletebutton", false);
            if (!FullScreenButton)
                output.Attributes.Add("data-widget-fullscreenbutton", false);

            TagBuilder header = new TagBuilder("header");
            if(!string.IsNullOrEmpty(Icon))
            {
                header.InnerHtml.AppendHtml($"<span class=\"widget-icon\"><i class=\"fa {Icon}\"></i></span>");
            }
            header.InnerHtml.AppendHtml($"<h2>{Name}</h2>");
            output.Content.AppendHtml(header);

            TagBuilder innerDiv = new TagBuilder("div");
            if (Editable)
            {
                TagBuilder editBox = new TagBuilder("div");
                editBox.AddCssClass("jarviswidget-editbox");
                editBox.InnerHtml.AppendHtml("<input class=\"form-control\" type=\"text\">");
                editBox.InnerHtml.AppendHtml("<span class=\"note\"><i class=\"fa fa-check text-success\"></i> Change title to update and save instantly!</span>");
                innerDiv.InnerHtml.AppendHtml(editBox);
            }

            TagBuilder widgetBody = new TagBuilder("div");
            widgetBody.AddCssClass("widget-body");
            if (!BodyPaddings)
                widgetBody.AddCssClass("no-padding");
            widgetBody.InnerHtml.AppendHtml(await output.GetChildContentAsync());

            innerDiv.InnerHtml.AppendHtml(widgetBody);
            output.Content.AppendHtml(innerDiv);
        }
    }
}
