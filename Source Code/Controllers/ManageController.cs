using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoneyTrackr.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        public PartialViewResult Index()
        {
            return PartialView("_IndexPartial");
        }
    }
}