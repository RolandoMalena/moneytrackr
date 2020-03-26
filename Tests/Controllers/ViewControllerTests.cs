using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;
using static MoneyTrackr.Constants.Role;
using static MoneyTrackr.Constants.User;

namespace MoneyTrackr.Tests.Controllers
{
    [TestFixture]
    public class ViewControllerTests : TestBase
    {
        const string homeEndpoint = "/Home/";
        const string homeEndpoint_GetHomePage = homeEndpoint + "GetHomePage";
        const string homeEndpoint_GetAboutPage = homeEndpoint + "GetAboutPage";
        const string homeEndpoint_GetNotFoundPage = homeEndpoint + "GetNotFoundPage";

        const string manageEndpoint = "/Manage/";

        const string usersEndpoint = "/Users/";
        const string usersEndpoint_GetRow = usersEndpoint + "GetRow";
        const string usersEndpoint_GetForm = usersEndpoint + "GetForm";

        const string entriesEndpoint = "/Entries/";
        const string entriesEndpoint_GetRow = entriesEndpoint + "GetRow";
        const string entriesEndpoint_GetForm = entriesEndpoint + "GetForm";

        const string swaggerEndpoint = "/api/doc/index.html";
        
        [Test]
        [TestCase(null, homeEndpoint, @"<div id=""content"">", @"<div id=""loading""")]
        [TestCase(null, homeEndpoint_GetHomePage, @">Welcome to MoneyTrackr!</h1>", @"<form id=""loginForm"" onsubmit=", @"<form id=""registerForm"" onsubmit=")]
        [TestCase(null, homeEndpoint_GetAboutPage, @">About the Developer</h1>")]
        [TestCase(null, homeEndpoint_GetNotFoundPage, @"<img src=""/images/404.png"" />")]
        [TestCase(null, swaggerEndpoint, @"""url"":""/swagger/v1/swagger.json""", @"""name"":""MoneyTrackr V1""")]
        [TestCase(UserType.Administrator, manageEndpoint, @"<form id=""changeUsernameForm"" onsubmit=", @"<form id=""changePasswordForm"" onsubmit=")]
        [TestCase(UserType.UserManager, manageEndpoint, @"<form id=""changeUsernameForm"" onsubmit=", @"<form id=""changePasswordForm"" onsubmit=")]
        [TestCase(UserType.Regular, manageEndpoint, @"<form id=""changeUsernameForm"" onsubmit=", @"<form id=""changePasswordForm"" onsubmit=")]
        [TestCase(UserType.Administrator, usersEndpoint, @">Manage Users</h1>", @"<div id=""modal""")]
        [TestCase(UserType.UserManager, usersEndpoint, @">Manage Users</h1>", @"<div id=""modal""")]
        [TestCase(UserType.Administrator, usersEndpoint_GetRow, @"<tr name=""{UserName}"">")]
        [TestCase(UserType.UserManager, usersEndpoint_GetRow, @"<tr name=""{UserName}"">")]
        [TestCase(UserType.Administrator, usersEndpoint_GetForm, @"<form id=""usersForm"" onsubmit", AdministratorRoleName + "</option>", UserManagerRoleName + "</option>", RegularUserRoleName +  "</option>")]
        [TestCase(UserType.UserManager, usersEndpoint_GetForm, @"<form id=""usersForm"" onsubmit", UserManagerRoleName + "</option>", RegularUserRoleName + "</option>")]
        [TestCase(UserType.Administrator, entriesEndpoint, @">Manage Entries</h1>", @"<div id=""modal""", @"<form id=""searchForm""", AdminUserName + "</option>")]
        [TestCase(UserType.UserManager, entriesEndpoint, @">Manage Entries</h1>", @"<div id=""modal""", @"<form id=""searchForm""", ManagerUserName + "</option>")]
        [TestCase(UserType.Regular, entriesEndpoint, @">Manage Entries</h1>", @"<div id=""modal""", @"<form id=""searchForm""", RegularUserName + "</option>")]
        [TestCase(UserType.Administrator, entriesEndpoint_GetRow, @"<tr name=""{Id}"">")]
        [TestCase(UserType.UserManager, entriesEndpoint_GetRow, @"<tr name=""{Id}"">")]
        [TestCase(UserType.Regular, entriesEndpoint_GetRow, @"<tr name=""{Id}"">")]
        [TestCase(UserType.Administrator, entriesEndpoint_GetForm, @"<form id=""entriesForm"" onsubmit")]
        [TestCase(UserType.UserManager, entriesEndpoint_GetForm, @"<form id=""entriesForm"" onsubmit")]
        [TestCase(UserType.Regular, entriesEndpoint_GetForm, @"<form id=""entriesForm"" onsubmit")]
        public async Task GetPage_ShouldContainExpectedContent(UserType? userType, string url, params string[] contentToExpect)
        {
            if (userType.HasValue)
                SetToken(userType.Value);
            else
                RemoveToken();

            var response = await Client.GetAsync(url);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            string html = await response.Content.ReadAsStringAsync();
            foreach (string notExpectedContent in contentToExpect)
                StringAssert.Contains(notExpectedContent, html);
        }

        [Test]
        [TestCase(UserType.UserManager, usersEndpoint_GetForm, AdministratorRoleName + "</option>")]
        public async Task GetPage_ShouldNotContainContent(UserType? userType, string url, params string[] contentToNotExpect)
        {
            if (userType.HasValue)
                SetToken(userType.Value);
            else
                RemoveToken();

            var response = await Client.GetAsync(url);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            string html = await response.Content.ReadAsStringAsync();
            foreach (string expectedContent in contentToNotExpect)
                StringAssert.DoesNotContain(expectedContent, html);
        }

        [Test]
        [TestCase(manageEndpoint)]
        [TestCase(usersEndpoint)]
        [TestCase(entriesEndpoint)]
        public async Task GetPageAsAnonymous_ShouldReturnUnauthorized(string url)
        {
            RemoveToken();

            var response = await Client.GetAsync(url);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase(usersEndpoint)]
        [TestCase(usersEndpoint_GetRow)]
        [TestCase(usersEndpoint_GetForm)]
        public async Task GetPageAsRegularUser_ShouldReturnForbidden(string url)
        {
            SetToken(UserType.Regular);

            var response = await Client.GetAsync(url);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
