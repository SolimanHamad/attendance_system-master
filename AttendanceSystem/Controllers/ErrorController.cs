using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            if (statusCode == 404) 
                ViewBag.ErrorMessage = "Page Not Found!";
            return View("Error");
        }
    }
}
