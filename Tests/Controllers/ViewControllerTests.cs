using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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

        const string identitySufix = "/Identity/";
        const string identityAccountSufix = "/Identity/Account/";
        const string identityManageSufix = "/Identity/Account/Manage/";

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

        [Test]
        [TestCase("GET", identitySufix, "Error")]

        //Test cases for the Account pages
        [TestCase("GET", identityAccountSufix, "AccessDenied")]
        [TestCase("GET", identityAccountSufix, "ConfirmEmail")]
        [TestCase("GET", identityAccountSufix, "ConfirmEmailChange")]
        [TestCase("GET", identityAccountSufix, "ExternalLogin")]
        [TestCase("POST", identityAccountSufix, "ExternalLogin")]
        [TestCase("POST", identityAccountSufix, "ForgotPassword")]
        [TestCase("GET", identityAccountSufix, "ForgotPasswordConfirmation")]
        [TestCase("GET", identityAccountSufix, "Lockout")]
        [TestCase("GET", identityAccountSufix, "Login")]
        [TestCase("POST", identityAccountSufix, "Login")]
        [TestCase("GET", identityAccountSufix, "LoginWith2fa")]
        [TestCase("POST", identityAccountSufix, "LoginWith2fa")]
        [TestCase("GET", identityAccountSufix, "LoginWithRecoveryCode")]
        [TestCase("POST", identityAccountSufix, "LoginWithRecoveryCode")]
        [TestCase("GET", identityAccountSufix, "Logout")]
        [TestCase("POST", identityAccountSufix, "Logout")]
        [TestCase("GET", identityAccountSufix, "Register")]
        [TestCase("POST", identityAccountSufix, "Register")]
        [TestCase("GET", identityAccountSufix, "RegisterConfirmation")]
        [TestCase("GET", identityAccountSufix, "ResetPassword")]
        [TestCase("POST", identityAccountSufix, "ResetPassword")]
        [TestCase("GET", identityAccountSufix, "ResetPasswordConfirmation")]

        //Test cases for the Manage pages
        [TestCase("GET", identityManageSufix, "ChangePassword")]
        [TestCase("POST", identityManageSufix, "ChangePassword")]
        [TestCase("GET", identityManageSufix, "DeletePersonalData")]
        [TestCase("POST", identityManageSufix, "DeletePersonalData")]
        [TestCase("GET", identityManageSufix, "Disable2fa")]
        [TestCase("POST", identityManageSufix, "Disable2fa")]
        [TestCase("POST", identityManageSufix, "DownloadPersonalData")]
        [TestCase("GET", identityManageSufix, "Email")]
        [TestCase("GET", identityManageSufix, "EnableAuthenticator")]
        [TestCase("POST", identityManageSufix, "EnableAuthenticator")]
        [TestCase("GET", identityManageSufix, "ExternalLogins")]
        [TestCase("GET", identityManageSufix, "GenerateRecoveryCodes")]
        [TestCase("POST", identityManageSufix, "GenerateRecoveryCodes")]
        [TestCase("GET", identityManageSufix, "Index")]
        [TestCase("POST", identityManageSufix, "Index")]
        [TestCase("GET", identityManageSufix, "PersonalData")]
        [TestCase("GET", identityManageSufix, "ResetAuthenticator")]
        [TestCase("POST", identityManageSufix, "ResetAuthenticator")]
        [TestCase("GET", identityManageSufix, "SetPassword")]
        [TestCase("POST", identityManageSufix, "SetPassword")]
        [TestCase("GET", identityManageSufix, "ShowRecoveryCodes")]
        [TestCase("GET", identityManageSufix, "TwoFactorAuthentication")]
        [TestCase("POST", identityManageSufix, "TwoFactorAuthentication")]

        public async Task AccessIdentityEndpoint_ShouldRedirectToNotFoundPage(string method, string sufix, string endpoint)
        {
            var request = new HttpRequestMessage();
            HttpMethod m = null;

            switch (method)
            {
                case "GET": m = HttpMethod.Get; break;
                case "POST": m = HttpMethod.Post; break;
            }
            Assert.IsNotNull(m, $"Http Method {method} is not supported.");

            request.Method = m;
            request.RequestUri = new Uri(sufix + endpoint, UriKind.Relative);

            if(method == "POST")
            {
                string body = GetRequestBody(endpoint);

                if (body != null)
                    request.Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
            }

            var response = await Client.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
            Assert.IsNotNull(response.Headers);
            Assert.IsNotNull(response.Headers.Location);
            Assert.AreEqual("/#NotFound", response.Headers.Location.OriginalString);
        }

        private string GetRequestBody(string endpoint)
        {
            Dictionary<string, string> pairs = null;

            switch (endpoint)
            {
                case "ExternalLogin":
                    pairs = new Dictionary<string, string>()
                    {
                        { "Input.Email", "test@test.com" }
                    };
                    break;

                case "ForgotPassword":
                    pairs = new Dictionary<string, string>()
                    {
                        { "Input.Email", "test@test.com" }
                    };
                    break;

                case "Disable2fa":
                    pairs = new Dictionary<string, string>()
                    {
                        { "Input.TwoFactorCode", "asd-123" }
                    };
                    break;

                case "Login":
                    pairs = new Dictionary<string, string>()
                    {
                        { "Input.Email", "test@test.com" },
                        { "Input.Password", "Asd-123" }
                    };
                    break;

                case "LoginWithRecoveryCode":
                    pairs = new Dictionary<string, string>()
                    {
                        { "Input.RecoveryCode", "asd123" }
                    };
                    break;

                case "Register":
                    pairs = new Dictionary<string, string>()
                    {
                        { "Input.Email", "test@test.com" },
                        { "Input.Password", "Asd-123" },
                        { "Input.ConfirmPassword", "Asd-123" }
                    };
                    break;

                case "ResetPassword":
                    pairs = new Dictionary<string, string>()
                    {
                        { "Input.Email", "test@test.com" },
                        { "Input.Password", "Asd-123" },
                        { "Input.ConfirmPassword", "Asd-123" }
                    };
                    break;

                case "ChangePassword":
                    pairs = new Dictionary<string, string>()
                    {
                        { "Input.OldPassword", "Asd-123" },
                        { "Input.NewPassword", "Asd-123" },
                        { "Input.ConfirmPassword", "Asd-123" }
                    };
                    break;

                case "DeletePersonalData":
                    pairs = new Dictionary<string, string>()
                    {
                        { "Input.Password", "Asd-123" }
                    };
                    break;

                case "EnableAuthenticator":
                    pairs = new Dictionary<string, string>()
                    {
                        { "Input.Code", "asd123" }
                    };
                    break;

                case "SetPassword":
                    pairs = new Dictionary<string, string>()
                    {
                        { "Input.NewPassword", "Asd-123" },
                        { "Input.ConfirmPassword", "Asd-123" }
                    };
                    break;

                default:
                    return null;
            }

            return string.Join("&", pairs.Select(kvp => kvp.Key + kvp.Value));
        }
    }
}
