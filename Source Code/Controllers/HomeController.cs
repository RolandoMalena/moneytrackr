using Microsoft.AspNetCore.Mvc;

namespace MoneyTrackr.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        //Initial Landing Page
        public ActionResult Index()
        {
            return View();
        }

        //Returns the Home Page for non-authenticated Users
        public PartialViewResult GetHomePage()
        {
            return PartialView("_Home");
        }

        //Returns the About Page
        public PartialViewResult GetAboutPage()
        {
            return PartialView("_About");
        }

        //Returns the NotFound Page
        public PartialViewResult GetNotFoundPage()
        {
            return PartialView("_NotFoundPartial");
        }
    }
}
