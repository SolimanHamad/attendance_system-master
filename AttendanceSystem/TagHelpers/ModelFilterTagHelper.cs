using AttendanceSystem.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using AttendanceSystem.Data.QueryFilter;
using System.Collections.Generic;
using System.Globalization;

namespace AttendanceSystem.TagHelpers
{
    [HtmlTargetElement("filter", Attributes = "for")]
    public class ModelFilterTagHelper : TagHelper
    {
        [HtmlAttributeName("for")]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("comparison")]
        public ComparisonType Comparison { get; set; }

        [HtmlAttributeName("icon")]
        public string Icon { get; set; }
        
        [HtmlAttributeName("range-limit")]
        public TimeSpan? RangeLimit { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            TagBuilder input;
            if (For.ModelExplorer.ModelType == typeof(bool) || For.ModelExplorer.ModelType.IsEnum)
            {
                input = new TagBuilder("select");
                input.Attributes.Add("type", "text");
                input.Attributes.Add("data-property", For.Metadata.PropertyName);
                input.Attributes.Add("data-comparison", Comparison.ToString());
                input.Attributes.Add("data-filter", null);
                input.Attributes.Add("onchange", "$('table').DataTable().ajax.reload();");
                input.AddCssClass("form-control");
                List<TagBuilder> options = new List<TagBuilder>();
                // Add default option
                TagBuilder defaultOption = new TagBuilder("option");
                defaultOption.Attributes.Add("value", null);
                defaultOption.InnerHtml.Append(For.Metadata.DisplayOrName());
                options.Add(defaultOption);
                
                if (For.ModelExplorer.ModelType.IsEnum)
                {
                    string[] names = Enum.GetNames(For.ModelExplorer.ModelType);
                    for (int i = 0; i < names.Length; i++)
                    {
                        TagBuilder option = new TagBuilder("option");
                        option.Attributes.Add("value", ((int)Enum.Parse(For.ModelExplorer.ModelType, names[i])).ToString());
                        option.InnerHtml.Append(names[i]);
                        options.Add(option);
                    }
                }
                else
                {
                    TagBuilder yesOption = new TagBuilder("option");
                    yesOption.Attributes.Add("value", "true");
                    yesOption.InnerHtml.Append("Yes");
                    options.Add(yesOption);
                    TagBuilder noOption = new TagBuilder("option");
                    noOption.Attributes.Add("value", "false");
                    noOption.InnerHtml.Append("No");
                    options.Add(noOption);
                }
                
                foreach(TagBuilder option in options)
                    input.InnerHtml.AppendHtml(option);
            }
            else
            {
                input = new TagBuilder("input");
                input.Attributes.Add("type", "text");
                input.Attributes.Add("placeholder", For.Metadata.DisplayOrName());
                input.Attributes.Add("data-property", For.Metadata.PropertyName);
                input.Attributes.Add("data-comparison", Comparison.ToString());
                input.Attributes.Add("data-filter", null);
                input.Attributes.Add("onchange","$('table').DataTable().ajax.reload();");
                // Configure Data for daterangepickers
                if (For.ModelExplorer.ModelType == typeof(DateTime))
                {
                    input.Attributes.Add("daterangepicker", null);
                    if(RangeLimit != null)
                        input.Attributes.Add("data-limit_in_minutes", RangeLimit.Value.TotalMinutes.ToString(CultureInfo.InvariantCulture));
                }
            }            

            TagBuilder col = new TagBuilder("div");
            TagBuilder label = new TagBuilder("label");
            TagBuilder icon = new TagBuilder("i");

            icon.AddCssClass("icon-append");
            icon.AddCssClass("fa");
            icon.AddCssClass(GetIcon());
            label.AddCssClass("input");

            label.InnerHtml.AppendHtml(icon);
            label.InnerHtml.AppendHtml(input);

            col.InnerHtml.AppendHtml(label);
            output.Content.SetHtmlContent(col);
        }

        private string GetIcon()
        {
            if (!string.IsNullOrEmpty(Icon))
                return Icon;
            if(For.ModelExplorer.ModelType == typeof(DateTime))
                return "fa-calendar";
            if (For.Metadata.PropertyName.Contains("ID"))
                return "fa-id-badge";
            if (For.Metadata.PropertyName.Contains("Email"))
                return "fa-envelope";
            return "";
        }
    }
}
