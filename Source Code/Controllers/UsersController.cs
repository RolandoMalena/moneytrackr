using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static MoneyTrackr.Constants.Role;

namespace MoneyTrackr.Controllers
{
    //Only allow users with Administrator and User Manager roles
    [Authorize(Roles = AdministratorRoleName + "," + UserManagerRoleName)]
    public class UsersController : Controller
    {
        public PartialViewResult Index()
        {
            return PartialView("_IndexPartial");
        }
    }
}