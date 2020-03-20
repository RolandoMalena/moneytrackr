using MoneyTrackr.Dtos;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;
using static MoneyTrackr.Constants.User;

namespace MoneyTrackr.Tests.Controllers.API
{
    [TestFixture]
    public class ManageControllerTests : TestBase
    {
        const string changeUsernameEndpoint = "/api/Manage/ChangeUsername";
        const string changePasswordEndpoint = "/api/Manage/ChangePassword";

        ChangeUserNameDto changeUserNameDto = new ChangeUserNameDto();
        ChangePasswordDto changePasswordDto = new ChangePasswordDto();

        #region ChangeUsername
        [Test]
        public async Task ChangeUsernameAsAnonymous_ShouldReturnUnauthorized()
        {
            RemoveToken();

            var response = await Client.PatchAsync(changeUsernameEndpoint, "".ToHttpContent());

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase("", "")]
        [TestCase("testing", "")]
        [TestCase("", "testing1")]
        public async Task ChangeUsernameAsAuthenticatedUserWithWrongData_ShouldReturnBadRequest(string username, string password)
        {
            SetToken(UserType.Regular);
            changeUserNameDto.UserName = username;
            changeUserNameDto.CurrentPassword = password;

            var response = await Client.PatchAsync(changeUsernameEndpoint, changeUserNameDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            StringAssert.Contains(ValidationError, await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task ChangeUsernameAsAuthenticatedUserWithWrongPassword_ShouldReturnBadRequest()
        {
            SetToken(UserType.Regular);
            changeUserNameDto.UserName = RegularUserName;
            changeUserNameDto.CurrentPassword = "test";

            var response = await Client.PatchAsync(changeUsernameEndpoint, changeUserNameDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            Assert.AreEqual("The Current Password is incorrect.", await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task ChangeUsernameAsAuthenticatedUserWithCorrectPassword_ShouldReturnOk()
        {
            SetToken(UserType.Regular);
            changeUserNameDto.UserName = RegularUserName;
            changeUserNameDto.CurrentPassword = Configuration["Passwords:RegularPassword"];

            var response = await Client.PatchAsync(changeUsernameEndpoint, changeUserNameDto.ToHttpContent());

            Assert.IsTrue(response.IsSuccessStatusCode);
        }
        #endregion

        #region ChangePassword
        [Test]
        public async Task ChangePasswordAsAnonymous_ShouldReturnUnauthorized()
        {
            RemoveToken();

            var response = await Client.PatchAsync(changePasswordEndpoint, "".ToHttpContent());

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase("", "")]
        [TestCase("testing1", "")]
        [TestCase("", "testing1")]
        public async Task ChangePasswordAsAuthenticatedUserWithWrongData_ShouldReturnBadRequest(string currentPassword, string newPassword)
        {
            SetToken(UserType.Regular);
            changePasswordDto.CurrentPassword = currentPassword;
            changePasswordDto.NewPassword = newPassword;

            var response = await Client.PatchAsync(changePasswordEndpoint, changePasswordDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            StringAssert.Contains(ValidationError, await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task ChangePasswordAsAuthenticatedUserWithWrongPassword_ShouldReturnBadRequest()
        {
            SetToken(UserType.Regular);
            changePasswordDto.CurrentPassword = "test";
            changePasswordDto.NewPassword = "testing1";

            var response = await Client.PatchAsync(changePasswordEndpoint, changePasswordDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            Assert.AreEqual("The Current Password is incorrect.", await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task ChangePasswordAsAuthenticatedUserWithCorrectPassword_ShouldReturnOk()
        {
            SetToken(UserType.Regular);
            changePasswordDto.CurrentPassword = Configuration["Passwords:RegularPassword"];
            changePasswordDto.NewPassword = Configuration["Passwords:RegularPassword"];

            var response = await Client.PatchAsync(changePasswordEndpoint, changePasswordDto.ToHttpContent());

            Assert.IsTrue(response.IsSuccessStatusCode);
        }
        #endregion
    }
}
