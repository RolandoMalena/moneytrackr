using MoneyTrackr.Dtos;
using NUnit.Framework;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static MoneyTrackr.Constants.Role;

namespace MoneyTrackr.Tests.Controllers.API
{
    [TestFixture]
    public class RolesControllerTests : TestBase
    {
        const string endpoint = "/api/Roles";

        #region Get
        [Test]
        public async Task GetRolesAsAnonymous_ShouldGetUnauthorized()
        {
            RemoveToken();

            var response = await Client.GetAsync(endpoint);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task GetRolesAsAdministrator_ShouldGetAdministratorRoles()
        {
            SetToken(UserType.Administrator);

            var response = await Client.GetAsync(endpoint);

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var roles = await response.Content.Deserialize<RoleDto[]>();
            Assert.IsNotNull(roles);
            Assert.IsNotNull(roles.SingleOrDefault(r => r.Id == AdministratorRoleId));
        }

        [Test]
        public async Task GetRolesAsUserManager_ShouldNotGetAdministratorRole()
        {
            SetToken(UserType.UserManager);

            var response = await Client.GetAsync(endpoint);

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var roles = await response.Content.Deserialize<RoleDto[]>();
            Assert.IsNotNull(roles);
            Assert.IsNull(roles.SingleOrDefault(r => r.Id == AdministratorRoleId));
        }

        [Test]
        public async Task GetRolesAsRegularUser_ShouldGetForbidden()
        {
            SetToken(UserType.Regular);

            var response = await Client.GetAsync(endpoint);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion
    }
}
