using Microsoft.AspNetCore.Mvc;

namespace MoneyTrackr.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult GetHomePage()
        {
            return PartialView("_Home");
        }

        public PartialViewResult GetAboutPage()
        {
            return PartialView("_About");
        }

        public PartialViewResult GetNotFoundPage()
        {
            return PartialView("_NotFoundPartial");
        }

        [Route("Error/{StatusCode}")]
        public IActionResult StatusCodeHandle(int statusCode)
        {
            if(statusCode == 404)
                return Redirect("~/#NotFound");

            return StatusCode(statusCode);
        }
    }
}
