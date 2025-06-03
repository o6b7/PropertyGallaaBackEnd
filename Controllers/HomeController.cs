using Microsoft.AspNetCore.Mvc;

namespace PropertyGalla.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
