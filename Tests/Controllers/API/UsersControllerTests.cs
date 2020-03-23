using MoneyTrackr.Dtos;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static MoneyTrackr.Constants.Role;
using static MoneyTrackr.Constants.User;

namespace MoneyTrackr.Tests.Controllers.API
{
    [TestFixture]
    public class UsersControllerTests : TestBase
    {
        const string usersEndpoint = "/api/Users/";
        const string loginEndpoint = "/api/Users/Login";
        const string registerEndpoint = "/api/Users/Register";
        const string getByRoleEndpoint = "/api/Users/GetByRole/";

        const string loginFailedError = "Username or Password is incorrect.";

        UserDto userDto = new UserDto();
        UserLogInDto logInDto = new UserLogInDto();
        RegisterUserDto registerDto = new RegisterUserDto();

        #region Login
        [Test]
        [TestCase("", "")]
        [TestCase("testing", "")]
        [TestCase("", "testing1")]
        public async Task LoginWithWrongData_ShouldReturnBadRequest(string username, string password)
        {
            RemoveToken();
            logInDto.Username = username;
            logInDto.Password = password;

            var response = await Client.PostAsync(loginEndpoint, logInDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            StringAssert.Contains(ValidationError, await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task LoginWithNonExistantUsername_ShouldReturnBadRequest()
        {
            RemoveToken();
            logInDto.Username = "testing";
            logInDto.Password = "testing1";

            var response = await Client.PostAsync(loginEndpoint, logInDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            Assert.AreEqual(loginFailedError, await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task LoginWithWrongPassword_ShouldReturnBadRequest()
        {
            RemoveToken();
            logInDto.Username = RegularUserName;
            logInDto.Password = "testing1";

            var response = await Client.PostAsync(loginEndpoint, logInDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            Assert.AreEqual(loginFailedError, await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task LoginWithWrongPasswordMultipleTimes_ShouldReturnBadRequestWithBlockedUserMessage()
        {
            RemoveToken();
            logInDto.Username = ManagerUserName;
            logInDto.Password = "testing1";

            HttpResponseMessage response = null;

            for (int i = 0; i < 4; i++)
            {
                response = await Client.PostAsync(loginEndpoint, logInDto.ToHttpContent());
                Assert.IsFalse(response.IsSuccessStatusCode);
            }

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            Assert.AreEqual("User has been blocked out, try again after 5 minutes have passed.", await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task LoginWithCorrectPassword_ShouldReturnOkWithJWT()
        {
            RemoveToken();
            logInDto.Username = RegularUserName;
            logInDto.Password = Configuration["Passwords:RegularPassword"];

            var response = await Client.PostAsync(loginEndpoint, logInDto.ToHttpContent());

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var tokenContainer = await response.Content.Deserialize<Dictionary<string, string>>();
            Assert.IsNotNull(tokenContainer);
            Assert.IsTrue(tokenContainer.ContainsKey("auth_token"));
        }
        #endregion

        #region Register
        [Test]
        [TestCase("", "")]
        [TestCase("testing", "")]
        [TestCase("", "testing1")]
        public async Task RegisterWithWrongData_ShouldReturnBadRequest(string username, string password)
        {
            RemoveToken();
            registerDto.UserName = username;
            registerDto.Password = password;

            var response = await Client.PostAsync(registerEndpoint, registerDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            StringAssert.Contains(ValidationError, await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task RegisterWithExistentUsername_ShouldReturnBadRequest()
        {
            RemoveToken();
            registerDto.UserName = RegularUserName;
            registerDto.Password = "testing1";

            var response = await Client.PostAsync(registerEndpoint, registerDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            Assert.AreEqual($"User name '{RegularUserName}' is already taken.", await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task RegisterWithValidData_ShouldReturnOkAndGetRegisteredUser()
        {
            RemoveToken();

            //Register
            registerDto.UserName = "RegisterTestUser";
            registerDto.Password = "password1";

            var response = await Client.PostAsync(registerEndpoint, registerDto.ToHttpContent());

            Assert.IsTrue(response.IsSuccessStatusCode);

            //Login
            logInDto.Username = registerDto.UserName;
            logInDto.Password = registerDto.Password;

            response = await Client.PostAsync(loginEndpoint, logInDto.ToHttpContent());

            Assert.IsTrue(response.IsSuccessStatusCode);

            var tokenContainer = await response.Content.Deserialize<Dictionary<string, string>>();
            Assert.IsNotNull(tokenContainer);
            Assert.IsTrue(tokenContainer.ContainsKey("auth_token"));
        }
        #endregion

        #region GetAll
        [Test]
        public async Task GetAllUsersAsAnonymous_ShouldGetUnauthorized()
        {
            RemoveToken();

            var response = await Client.GetAsync(usersEndpoint);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task GetAllUsersAsAdministrator_ShouldGetUsersWithAdministratorRole()
        {
            SetToken(UserType.Administrator);

            var response = await Client.GetAsync(usersEndpoint);

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var users = await response.Content.Deserialize<UserDto[]>();
            Assert.IsNotNull(users);
            Assert.IsNotNull(users.Where(u => u.Role.Id == AdministratorRoleId).FirstOrDefault());
        }

        [Test]
        public async Task GetAllUsersAsUserManager_ShouldNotGetUsersWithAdministratorRole()
        {
            SetToken(UserType.UserManager);

            var response = await Client.GetAsync(usersEndpoint);

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var users = await response.Content.Deserialize<UserDto[]>();
            Assert.IsNotNull(users);
            Assert.IsNull(users.Where(u => u.Role.Id == AdministratorRoleId).FirstOrDefault());
        }

        [Test]
        public async Task GetAllUsersAsRegularUser_ShouldGetForbidden()
        {
            SetToken(UserType.Regular);

            var response = await Client.GetAsync(usersEndpoint);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        #region Get
        [Test]
        public async Task GetUserAsAnonymous_ShouldGetUnauthorized()
        {
            RemoveToken();

            var response = await Client.GetAsync(usersEndpoint + RegularUserName);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase(AdminUserName)]
        [TestCase(ManagerUserName)]
        [TestCase(RegularUserName)]
        public async Task GetUserAsAdministrator_ShouldGetUser(string username)
        {
            SetToken(UserType.Administrator);

            var response = await Client.GetAsync(usersEndpoint + username);

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var user = await response.Content.Deserialize<UserDto>();
            Assert.IsNotNull(user);
            Assert.IsNotNull(user.Role);
        }

        [Test]
        [TestCase(ManagerUserName)]
        [TestCase(RegularUserName)]
        public async Task GetUserAsUserManager_ShouldGetUser(string username)
        {
            SetToken(UserType.UserManager);

            var response = await Client.GetAsync(usersEndpoint + username);

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var user = await response.Content.Deserialize<UserDto>();
            Assert.IsNotNull(user);
            Assert.IsNotNull(user.Role);
        }

        [Test]
        public async Task GetAdministratorUserAsUserManager_ShouldNotGetUser()
        {
            SetToken(UserType.UserManager);

            var response = await Client.GetAsync(usersEndpoint + AdminUserName);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task GetNonExistingUserAsUserManager_ShouldReturnNotFound()
        {
            SetToken(UserType.UserManager);

            var response = await Client.GetAsync(usersEndpoint + "testing");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task GetUserAsRegularUser_ShouldGetForbidden()
        {
            SetToken(UserType.Regular);

            var response = await Client.GetAsync(usersEndpoint + RegularUserName);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        #region GetByRole
        [Test]
        public async Task GetUsersByRoleAsAnonymous_ShouldGetUnauthorized()
        {
            RemoveToken();

            var response = await Client.GetAsync(getByRoleEndpoint);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase(AdministratorRoleId)]
        [TestCase(UserManagerRoleId)]
        [TestCase(RegularUserRoleId)]
        public async Task GetUsersByRoleAsAdministrator_ShouldGetUsersWithAdministratorRole(string roleId)
        {
            SetToken(UserType.Administrator);

            var response = await Client.GetAsync(getByRoleEndpoint + roleId);

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var users = await response.Content.Deserialize<UserDto[]>();
            Assert.IsNotNull(users);
            Assert.IsTrue(users.Any());
        }

        [Test]
        [TestCase(UserManagerRoleId)]
        [TestCase(RegularUserRoleId)]
        public async Task GetUsersByRoleAsUserManager_ShouldGetUsers(string roleId)
        {
            SetToken(UserType.UserManager);

            var response = await Client.GetAsync(getByRoleEndpoint + roleId);

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var users = await response.Content.Deserialize<UserDto[]>();
            Assert.IsNotNull(users);
            Assert.IsTrue(users.Any());
        }

        [Test]
        public async Task GetUsersByRoleWithAdministratorRoleAsUserManager_ShouldGetForbidden()
        {
            SetToken(UserType.UserManager);

            var response = await Client.GetAsync(getByRoleEndpoint + AdministratorRoleId);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task GetUsersByRoleWithNonExistingRole_ShouldReturnBadRequest()
        {
            SetToken(UserType.UserManager);

            var response = await Client.GetAsync(getByRoleEndpoint + "testRole");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            Assert.AreEqual("The provided roleId is not valid", await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task GetUsersByRoleAsRegularUser_ShouldGetForbidden()
        {
            SetToken(UserType.Regular);

            var response = await Client.GetAsync(getByRoleEndpoint + RegularUserRoleId);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        #region Post
        [Test]
        public async Task PostUserAsAnonymous_ShouldGetUnauthorized()
        {
            RemoveToken();

            var response = await Client.PostAsync(usersEndpoint, userDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase("", "", "")]
        [TestCase("testing", "", "")]
        [TestCase("", "testing1", "")]
        [TestCase("", "", "testRole")]
        public async Task PostUserWithWrongData_ShouldReturnBadRequest(string username, string password, string roleId)
        {
            SetToken(UserType.Administrator);
            userDto.UserName = username;
            userDto.Password = password;
            userDto.RoleId = roleId;

            var response = await Client.PostAsync(usersEndpoint, userDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            StringAssert.Contains(ValidationError, await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task PostUserWithExistentUsername_ShouldReturnBadRequest()
        {
            SetToken(UserType.UserManager);
            userDto.UserName = RegularUserName;
            userDto.Password = "testing1";
            userDto.RoleId = RegularUserRoleId;

            var response = await Client.PostAsync(usersEndpoint, userDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            Assert.AreEqual($"User name '{RegularUserName}' is already taken.", await response.Content.ReadAsStringAsync());
        }

        [Test]
        [TestCase(AdministratorRoleId)]
        [TestCase(UserManagerRoleId)]
        [TestCase(RegularUserRoleId)]
        public async Task PostUserAsAdministrator_ShouldReturnCreated(string roleId)
        {
            SetToken(UserType.Administrator);
            userDto.UserName = GetRandomUsername();
            userDto.Password = "testing1";
            userDto.RoleId = roleId;

            //POST
            var response = await Client.PostAsync(usersEndpoint, userDto.ToHttpContent());
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            //GET
            response = await Client.GetAsync(usersEndpoint + userDto.UserName);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);
            var user = await response.Content.Deserialize<UserDto>();
            Assert.IsNotNull(user);
            Assert.IsNotNull(user.Role);
        }

        [Test]
        [TestCase(UserManagerRoleId)]
        [TestCase(RegularUserRoleId)]
        public async Task PostUserAsUserManager_ShouldReturnCreated(string roleId)
        {
            SetToken(UserType.UserManager);
            userDto.UserName = GetRandomUsername();
            userDto.Password = "testing1";
            userDto.RoleId = roleId;

            //POST
            var response = await Client.PostAsync(usersEndpoint, userDto.ToHttpContent());
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            //GET
            response = await Client.GetAsync(usersEndpoint + userDto.UserName);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);
            var user = await response.Content.Deserialize<UserDto>();
            Assert.IsNotNull(user);
            Assert.IsNotNull(user.Role);
        }

        [Test]
        public async Task PostUserWithAdministratorRoleAsUserManager_ShouldGetForbidden()
        {
            SetToken(UserType.UserManager);
            userDto.UserName = GetRandomUsername();
            userDto.Password = "testing1";
            userDto.RoleId = AdministratorRoleId;

            var response = await Client.PostAsync(usersEndpoint, userDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task PostUserAsRegularUser_ShouldGetForbidden()
        {
            SetToken(UserType.Regular);

            var response = await Client.GetAsync(usersEndpoint);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        #region Put
        [Test]
        public async Task PutUserAsAnonymous_ShouldGetUnauthorized()
        {
            RemoveToken();

            var response = await Client.PutAsync(usersEndpoint + RegularUserName, userDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase("", "", "")]
        [TestCase("testing", "", "")]
        [TestCase("", "testing1", "")]
        [TestCase("", "", "testRole")]
        public async Task PutUserWithWrongData_ShouldReturnBadRequest(string username, string password, string roleId)
        {
            SetToken(UserType.Administrator);
            userDto.UserName = username;
            userDto.Password = password;
            userDto.RoleId = roleId;

            var response = await Client.PutAsync(usersEndpoint + RegularUserName, userDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            StringAssert.Contains(ValidationError, await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task PutUserWithExistentUsername_ShouldReturnBadRequest()
        {
            SetToken(UserType.UserManager);
            userDto.UserName = ManagerUserName;
            userDto.Password = "testing1";
            userDto.RoleId = RegularUserRoleId;

            var response = await Client.PutAsync(usersEndpoint + RegularUserName, userDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            Assert.AreEqual($"User name '{ManagerUserName}' is already taken.", await response.Content.ReadAsStringAsync());
        }

        [Test]
        [TestCase(AdministratorRoleId)]
        [TestCase(UserManagerRoleId)]
        [TestCase(RegularUserRoleId)]
        public async Task PutUserAsAdministrator_ShouldReturnOk(string roleId)
        {
            SetToken(UserType.Administrator);
            userDto.UserName = GetRandomUsername();
            userDto.Password = "testing1";
            userDto.RoleId = RegularUserRoleId;

            //POST
            var response = await Client.PostAsync(usersEndpoint, userDto.ToHttpContent());
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            //PUT
            string oldUsername = userDto.UserName;
            userDto.UserName = GetRandomUsername();
            userDto.Password = "testing2";
            userDto.RoleId = roleId;

            response = await Client.PutAsync(usersEndpoint + oldUsername, userDto.ToHttpContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            //GET
            response = await Client.GetAsync(usersEndpoint + userDto.UserName);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);
            var user = await response.Content.Deserialize<UserDto>();
            Assert.IsNotNull(user);
            Assert.AreEqual(userDto.UserName, user.UserName);
            Assert.IsNotNull(user.Role);
            Assert.AreEqual(userDto.RoleId, user.Role.Id);

            //Login (to ensure the Password change)
            logInDto.Username = userDto.UserName;
            logInDto.Password = userDto.Password;

            response = await Client.PostAsync(loginEndpoint, logInDto.ToHttpContent());
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [Test]
        [TestCase(UserManagerRoleId)]
        [TestCase(RegularUserRoleId)]
        public async Task PutUserAsUserManager_ShouldReturnOk(string roleId)
        {
            SetToken(UserType.UserManager);
            userDto.UserName = GetRandomUsername();
            userDto.Password = "testing1";
            userDto.RoleId = RegularUserRoleId;

            //POST
            var response = await Client.PostAsync(usersEndpoint, userDto.ToHttpContent());
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            //PUT
            string oldUsername = userDto.UserName;
            userDto.UserName = GetRandomUsername();
            userDto.Password = "testing2";
            userDto.RoleId = roleId;

            response = await Client.PutAsync(usersEndpoint + oldUsername, userDto.ToHttpContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            //GET
            response = await Client.GetAsync(usersEndpoint + userDto.UserName);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);
            var user = await response.Content.Deserialize<UserDto>();
            Assert.IsNotNull(user);
            Assert.AreEqual(userDto.UserName, user.UserName);
            Assert.IsNotNull(user.Role);
            Assert.AreEqual(userDto.RoleId, user.Role.Id);

            //Login (to ensure the Password change)
            logInDto.Username = userDto.UserName;
            logInDto.Password = userDto.Password;

            response = await Client.PostAsync(loginEndpoint, logInDto.ToHttpContent());
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [Test]
        public async Task PutUserWithAdministratorRoleAsUserManager_ShouldGetForbidden()
        {
            SetToken(UserType.UserManager);
            userDto.UserName = "testing";
            userDto.RoleId = RegularUserRoleId;

            var response = await Client.PutAsync(usersEndpoint + AdminUserName, userDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task PutUserToAdministratorRoleAsUserManager_ShouldGetForbidden()
        {
            SetToken(UserType.UserManager);
            userDto.UserName = RegularUserName;
            userDto.RoleId = AdministratorRoleId;

            var response = await Client.PutAsync(usersEndpoint + userDto.UserName, userDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task PutUserAsRegularUser_ShouldGetForbidden()
        {
            SetToken(UserType.Regular);

            var response = await Client.GetAsync(usersEndpoint);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        #region Delete
        [Test]
        public async Task DeleteUserAsAnonymous_ShouldGetUnauthorized()
        {
            RemoveToken();

            var response = await Client.DeleteAsync(usersEndpoint + RegularUserName);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase(AdministratorRoleId)]
        [TestCase(UserManagerRoleId)]
        [TestCase(RegularUserRoleId)]
        public async Task DeleteUserAsAdministrator_ShouldDeleteUser(string roleId)
        {
            SetToken(UserType.Administrator);
            userDto.UserName = GetRandomUsername();
            userDto.Password = "testing1";
            userDto.RoleId = roleId;

            //POST
            var response = await Client.PostAsync(usersEndpoint, userDto.ToHttpContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            //DELETE
            response = await Client.DeleteAsync(usersEndpoint + userDto.UserName);
            Assert.IsTrue(response.IsSuccessStatusCode);

            //GET
            response = await Client.GetAsync(usersEndpoint + userDto.UserName);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        [TestCase(UserManagerRoleId)]
        [TestCase(RegularUserRoleId)]
        public async Task DeleteUserAsUserManager_ShouldDeleteUser(string roleId)
        {
            SetToken(UserType.UserManager);
            userDto.UserName = GetRandomUsername();
            userDto.Password = "testing1";
            userDto.RoleId = roleId;

            //POST
            var response = await Client.PostAsync(usersEndpoint, userDto.ToHttpContent());
            Assert.IsTrue(response.IsSuccessStatusCode);

            //DELETE
            response = await Client.DeleteAsync(usersEndpoint + userDto.UserName);
            Assert.IsTrue(response.IsSuccessStatusCode);

            //GET
            response = await Client.GetAsync(usersEndpoint + userDto.UserName);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task DeleteAdministratorUserAsUserManager_ShouldNotDeleteUser()
        {
            SetToken(UserType.UserManager);

            var response = await Client.DeleteAsync(usersEndpoint + AdminUserName);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task DeleteNonExistingUserAsUserManager_ShouldReturnNotFound()
        {
            SetToken(UserType.UserManager);

            var response = await Client.DeleteAsync(usersEndpoint + "testing");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task DeleteUserAsRegularUser_ShouldDeleteForbidden()
        {
            SetToken(UserType.Regular);

            var response = await Client.DeleteAsync(usersEndpoint + RegularUserName);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        #region Private Helpers
        private string GetRandomUsername()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 25);
        }
        #endregion
    }
}
