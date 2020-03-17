using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackr.ViewModels;

namespace MoneyTrackr.Controllers
{
    [Authorize]
    public class EntriesController : Controller
    {
        public PartialViewResult Index()
        {
            return PartialView("_IndexPartial");
        }

        public PartialViewResult GetRow()
        {
            return PartialView("_Row");
        }

        public PartialViewResult GetForm()
        {
            return PartialView("_FormPartial", new EntryViewModel());
        }
    }
}