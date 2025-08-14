using Microsoft.AspNetCore.Mvc;

namespace IncidentAPI.Controllers
{
    public class IncidentViewController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}