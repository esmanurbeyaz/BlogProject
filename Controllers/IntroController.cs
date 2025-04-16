using Microsoft.AspNetCore.Mvc;

namespace BlogProjectMVC2.Controllers
{
    public class IntroController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

