using AttendanceSystem.TagHelpers;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.ViewComponents
{
    public class DataTableViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(DataTableTagHelper tagHelper) => View(tagHelper);
    }
}
