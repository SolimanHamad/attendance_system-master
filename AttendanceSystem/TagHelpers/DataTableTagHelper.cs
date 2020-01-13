using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Linq;
using AttendanceSystem.ViewComponents;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using AttendanceSystem.Extensions;

namespace AttendanceSystem.TagHelpers
{
    [HtmlTargetElement("table", Attributes = "data-table")]
    [HtmlTargetElement("table", Attributes = "for")]
    public class DataTableTagHelper : TagHelper
    {
        [HtmlAttributeName("for")]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("paging")]
        public bool EnablePaging { get; set; } = true;

        [HtmlAttributeName("responsive")]
        public bool Responsive { get; set; } = false;

        [HtmlAttributeName("global-search")]
        public bool EnableGlobalSearch { get; set; } = true;

        [HtmlAttributeName("column-search")]
        public bool EnableColumnSearch { get; set; } = false;

        [HtmlAttributeName("column-reorder")]
        public bool EnableColumnReorder { get; set; } = true;

        [HtmlAttributeName("exporting")]
        public bool EnableExporting { get; set; } = true;

        [HtmlAttributeName("fixed-header")]
        public bool EnableFixedHeader { get; set; } = true;

        [HtmlAttributeName("group-on")]
        public int? GroupOn { get; set; }

        [HtmlAttributeName("ajax")]
        public string Ajax { get; set; }

        [HtmlAttributeName("auto-ajax")]
        public bool AutoAjax { get; set; } = true;

        public string TableID { get; set; }

        public List<ModelMetadata> AllowedProperties { get; private set; }
        public List<string> ColumnNames { get; private set; }

        [HtmlAttributeName("actions")]
        public List<CustomAction> CustomActions { get; set; } 

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        private readonly IViewComponentHelper _viewComponentHelper;

        public DataTableTagHelper(IViewComponentHelper viewComponentHelper)
        {
            _viewComponentHelper = viewComponentHelper;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if(For != null)
            {
                if(For.Metadata.ElementMetadata != null)
                    AllowedProperties = For.Metadata.ElementMetadata.Properties.Where(p => p.ShowForDisplay).ToList();
                else
                    AllowedProperties = For.Metadata.Properties.Where(p => p.ShowForDisplay).ToList();
                ColumnNames = AllowedProperties.Select(p => p.DisplayOrName()).ToList();
            }

            output.Attributes.RemoveAll("data-table");
            output.Attributes.TryGetAttribute("id", out TagHelperAttribute idAttribute);
            if (idAttribute == null)
            {
                TableID = "dt-" + context.UniqueId;
                output.Attributes.SetAttribute("id", TableID);
            }
            else
            {
                TableID = idAttribute.Value.ToString();
            }
            if (For != null)
                output.TagName = null; // remove the original tag, the View Component will generate the table
            output.Content.AppendHtml(await output.GetChildContentAsync());
            ((IViewContextAware)_viewComponentHelper).Contextualize(ViewContext);
            var content = await _viewComponentHelper.InvokeAsync(typeof(DataTableViewComponent), this);
            output.Content.AppendHtml(content);
        }
    }

    public class CustomAction
    {
        public string Name { get; set; }

        public string Icon { get; set; }

        public string Link { get; set; }

        public string ReturnUrl { get; set; }
    }
}


