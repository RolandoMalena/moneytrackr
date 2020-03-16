using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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