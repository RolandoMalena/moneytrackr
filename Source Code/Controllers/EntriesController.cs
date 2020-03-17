using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoneyTrackr.Controllers
{
    [Authorize]
    public class EntriesController : Controller
    {
        public PartialViewResult Index()
        {
            return PartialView("_IndexPartial");
        }
    }
}