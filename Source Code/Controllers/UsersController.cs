using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackr.ViewModels;
using System.Collections.Generic;
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

        public PartialViewResult GetRow()
        {
            return PartialView("_Row");
        }

        public PartialViewResult GetForm()
        {
            //Setup Roles to pass into the ViewModel
            var roles = new List<RoleViewModel>()
            {
                new RoleViewModel() { Id = UserManagerRoleId, Name = UserManagerRoleName },
                new RoleViewModel() { Id = RegularUserRoleId, Name = RegularUserRoleName }
            };

            //Insert Administrator Role if User is an Administrator
            if (User.IsInRole(AdministratorRoleName))
                roles.Insert(0, new RoleViewModel() { Id = AdministratorRoleId, Name = AdministratorRoleName });

            return PartialView("_FormPartial", new UserViewModel()
            {
                Roles = roles
            });
        }
    }
}