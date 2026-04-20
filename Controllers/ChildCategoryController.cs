using Microsoft.AspNetCore.Mvc;

namespace firstproject.Controllers
{
    public class ChildCategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
