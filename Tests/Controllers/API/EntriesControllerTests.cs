using Microsoft.AspNetCore.Identity;
using MoneyTrackr.Data;
using MoneyTrackr.Data.DomainObjects;
using MoneyTrackr.Dtos;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static MoneyTrackr.Constants.Role;
using static MoneyTrackr.Constants.User;
using static MoneyTrackr.Helpers.UserHelper;

namespace MoneyTrackr.Tests.Controllers.API
{
    [TestFixture]
    public class EntriesControllerTests : TestBase
    {
        const string endpoint = "api/Users/{0}/Entries/";
        const string endpointRegular = "api/Users/" + RegularUserName + "/Entries/";

        const string reportEndpoint = "api/Users/{0}/Entries/Report";
        const string reportEndpointRegular = "api/Users/" + RegularUserName + "/Entries/Report";

        const string filterParameters = "?from={0}&to={1}";

        const string alternateRegularUsername = "Regular2";

        EntryDto entryDto = new EntryDto();

        #region DB Seeding
        //Dictionary Structure: Method > UserType to test with > Username owner of the Entry > EntryId
        Dictionary<string, Dictionary<UserType, Dictionary<string, int>>> data;

        protected override void SeedDatabase(ApplicationDbContext dbContext)
        {
            int entryId = 0;
            List<Entry> entriesToAdd = new List<Entry>();
            Random random = new Random();

            //Start by creating new instances of the dictionaries and fill them
            data = new Dictionary<string, Dictionary<UserType, Dictionary<string, int>>>();

            foreach (string method in new string[] { "GET", "PUT", "DELETE" }) //Per each Method...
            {
                var userTypeDic = new Dictionary<UserType, Dictionary<string, int>>();
                foreach (string userType in Enum.GetNames(typeof(UserType))) //And per each UserType...
                {
                    var usernameDic = new Dictionary<string, int>();
                    var userTypeEnum = Enum.Parse<UserType>(userType);

                    //Add the Usernames based on the UserType
                    if (userTypeEnum == UserType.Administrator)
                    {
                        usernameDic.Add(AdminUserName, ++entryId);
                        usernameDic.Add(ManagerUserName, ++entryId);
                    }
                    else if (userTypeEnum == UserType.UserManager)
                    {
                        usernameDic.Add(ManagerUserName, ++entryId);
                    }
                    usernameDic.Add(RegularUserName, ++entryId);

                    userTypeDic.Add(userTypeEnum, usernameDic);
                }
                data.Add(method, userTypeDic);
            }

            //Create the Entries
            foreach (var methodsKvp in data)
            {
                foreach (var userTypeKvp in methodsKvp.Value)
                {
                    foreach (var kvp in userTypeKvp.Value)
                    {
                        entriesToAdd.Add(new Entry()
                        {
                            Id = kvp.Value,
                            Date = DateTime.Today,
                            Description = Guid.NewGuid().ToString(),
                            Amount = random.Next(1, 10) * 1000 * (random.Next(0, 2) == 0 ? 1 : -1),
                            UserId = GetUserId(kvp.Key)
                        });
                    }
                }
            }

            //Create Entries with a past date to test filtering
            foreach (string username in new string[] { AdminUserName, ManagerUserName, RegularUserName })
            {
                entriesToAdd.Add(new Entry()
                {
                    Id = ++entryId,
                    Date = DateTime.Today.AddMonths(-1),
                    Description = Guid.NewGuid().ToString(),
                    Amount = random.Next(1, 10) * 1000,
                    UserId = GetUserId(username)
                });
            }

            //Create another Regular User and add an Entry for them
            var user = new IdentityUser()
            {
                Id = Guid.NewGuid().ToString(),
                UserName = alternateRegularUsername,
                NormalizedUserName = alternateRegularUsername.ToUpper()
            };

            var userRole = new IdentityUserRole<string>()
            {
                UserId = user.Id,
                RoleId = RegularUserRoleId
            };

            entriesToAdd.Add(new Entry()
            {
                Id = ++entryId,
                Date = DateTime.Today,
                Description = Guid.NewGuid().ToString(),
                Amount = 1000,
                UserId = user.Id
            });

            //Add the rows and save the changes
            dbContext.Users.Add(user);
            dbContext.UserRoles.Add(userRole);
            dbContext.Entries.AddRange(entriesToAdd);
            dbContext.SaveChanges();
        }
        #endregion

        #region GetAll
        [Test]
        public async Task GetAllEntriesAsAnonymous_ShouldGetUnauthorized()
        {
            RemoveToken();

            var response = await Client.GetAsync(endpointRegular);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase(UserType.Administrator, AdminUserName)]
        [TestCase(UserType.Administrator, ManagerUserName)]
        [TestCase(UserType.Administrator, RegularUserName)]
        [TestCase(UserType.UserManager, ManagerUserName)]
        [TestCase(UserType.UserManager, RegularUserName)]
        [TestCase(UserType.Regular, RegularUserName)]
        public async Task GetAllEntriesForValidRoles_ShouldGetEntries(UserType userType, string username)
        {
            SetToken(userType);

            var response = await Client.GetAsync(endpoint.Format(username));

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var entries = await response.Content.Deserialize<EntryDto[]>();
            Assert.IsNotNull(entries);
            Assert.IsTrue(entries.Any());
        }

        [Test]
        [TestCase(UserType.Administrator, AdminUserName)]
        [TestCase(UserType.Administrator, ManagerUserName)]
        [TestCase(UserType.Administrator, RegularUserName)]
        [TestCase(UserType.UserManager, ManagerUserName)]
        [TestCase(UserType.UserManager, RegularUserName)]
        [TestCase(UserType.Regular, RegularUserName)]
        public async Task GetAllEntriesWithFiltering_ShouldGetFilteredEntries(UserType userType, string username)
        {
            SetToken(userType);
            DateTime dateToQuery = DateTime.Today.AddMonths(-1);
            string dateString = dateToQuery.ToString("yyyy-MM-dd");
            string url = endpoint.Format(username) + filterParameters.Format(dateString, dateString);

            var response = await Client.GetAsync(url);

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var entries = await response.Content.Deserialize<EntryDto[]>();
            Assert.IsNotNull(entries);
            Assert.IsTrue(entries.Any());

            foreach (var entry in entries)
                Assert.AreEqual(dateToQuery, entry.Date);
        }

        [Test]
        [TestCase(UserType.UserManager, AdminUserName)]
        [TestCase(UserType.Regular, AdminUserName)]
        [TestCase(UserType.Regular, ManagerUserName)]
        public async Task GetAllEntriesForInvalidRoles_ShouldGetForbidden(UserType userType, string username)
        {
            SetToken(userType);

            var response = await Client.GetAsync(endpoint.Format(username));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task GetAllEntriesForNonExistentUsername_ShouldGetNotFound()
        {
            SetToken(UserType.UserManager);

            var response = await Client.GetAsync(endpoint.Format("testUsername"));

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task GetAllEntriesForAnotherRegularUserAsRegularUser_ShouldGetForbidden()
        {
            SetToken(UserType.Regular);

            var response = await Client.GetAsync(endpoint.Format(alternateRegularUsername));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        #region Get
        [Test]
        public async Task GetEntryAsAnonymous_ShouldGetUnauthorized()
        {
            RemoveToken();

            var response = await Client.GetAsync(endpointRegular + int.MaxValue);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase(UserType.Administrator, AdminUserName)]
        [TestCase(UserType.Administrator, ManagerUserName)]
        [TestCase(UserType.Administrator, RegularUserName)]
        [TestCase(UserType.UserManager, ManagerUserName)]
        [TestCase(UserType.UserManager, RegularUserName)]
        [TestCase(UserType.Regular, RegularUserName)]
        public async Task GetEntryForValidRoles_ShouldGetEntry(UserType userType, string username)
        {
            SetToken(userType);
            int entryId = data["GET"][userType][username];

            var response = await Client.GetAsync(endpoint.Format(username) + entryId);

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var Entry = await response.Content.Deserialize<EntryDto>();
            Assert.IsNotNull(Entry);
        }

        [Test]
        [TestCase(UserType.UserManager, AdminUserName)]
        [TestCase(UserType.Regular, AdminUserName)]
        [TestCase(UserType.Regular, ManagerUserName)]
        public async Task GetEntryForInvalidRoles_ShouldGetForbidden(UserType userType, string username)
        {
            SetToken(userType);

            var response = await Client.GetAsync(endpoint.Format(username) + int.MaxValue);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        [TestCase(UserType.Administrator, AdminUserName)]
        [TestCase(UserType.UserManager, ManagerUserName)]
        [TestCase(UserType.Regular, RegularUserName)]
        public async Task GetNonExistentEntry_ShouldGetNotFound(UserType userType, string username)
        {
            SetToken(userType);

            var response = await Client.GetAsync(endpoint.Format(username) + int.MaxValue);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task GetEntryForNonExistentUsername_ShouldGetNotFound()
        {
            SetToken(UserType.UserManager);

            var response = await Client.GetAsync(endpoint.Format("testUsername") + int.MaxValue);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task GetEntryForAnotherRegularUserAsRegularUser_ShouldGetForbidden()
        {
            SetToken(UserType.Regular);
            entryDto.Description = "testing";
            entryDto.Amount = 1000;
            entryDto.Date = DateTime.Today;

            var response = await Client.GetAsync(endpoint.Format(alternateRegularUsername) + int.MaxValue);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        #region GetReport
        [Test]
        public async Task GetReportAsAnonymous_ShouldGetUnauthorized()
        {
            RemoveToken();

            var response = await Client.GetAsync(reportEndpointRegular);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase(UserType.Administrator, AdminUserName)]
        [TestCase(UserType.Administrator, ManagerUserName)]
        [TestCase(UserType.Administrator, RegularUserName)]
        [TestCase(UserType.UserManager, ManagerUserName)]
        [TestCase(UserType.UserManager, RegularUserName)]
        [TestCase(UserType.Regular, RegularUserName)]
        public async Task GetReportForValidRoles_ShouldGetEntries(UserType userType, string username)
        {
            SetToken(userType);

            var response = await Client.GetAsync(reportEndpoint.Format(username));

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var report = await response.Content.Deserialize<ReportDto>();
            Assert.IsNotNull(report);
            Assert.IsTrue(report.Entries.Any());

            //Ensure all the Summary data matches the returned Entries
            Assert.AreEqual(report.CurrentBalance, report.Entries.Sum(e => e.Amount));
            Assert.AreEqual(report.EntryCount, report.Entries.Length);
            Assert.AreEqual(report.TotalDeposits, report.Entries.Where(e => e.Amount > 0).Sum(e => e.Amount));
            Assert.AreEqual(report.DepositAverage, report.Entries.Where(e => e.Amount > 0).Average(e => e.Amount) ?? 0);
            Assert.AreEqual(report.DepositCount, report.Entries.Where(e => e.Amount > 0).Count());
            Assert.AreEqual(report.TotalWithdraws, report.Entries.Where(e => e.Amount < 0).Sum(e => e.Amount));
            Assert.AreEqual(report.WithdrawAverage, report.Entries.Where(e => e.Amount < 0).Average(e => e.Amount) ?? 0);
            Assert.AreEqual(report.WithdrawCount, report.Entries.Where(e => e.Amount < 0).Count());
        }

        [Test]
        [TestCase(UserType.Administrator, AdminUserName)]
        [TestCase(UserType.Administrator, ManagerUserName)]
        [TestCase(UserType.Administrator, RegularUserName)]
        [TestCase(UserType.UserManager, ManagerUserName)]
        [TestCase(UserType.UserManager, RegularUserName)]
        [TestCase(UserType.Regular, RegularUserName)]
        public async Task GetReportWithFiltering_ShouldGetReportWithFilteredEntries(UserType userType, string username)
        {
            SetToken(userType);
            DateTime dateToQuery = DateTime.Today.AddMonths(-1);
            string dateString = dateToQuery.ToString("yyyy-MM-dd");
            string url = reportEndpoint.Format(username) + filterParameters.Format(dateString, dateString);

            var response = await Client.GetAsync(url);

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var report = await response.Content.Deserialize<ReportDto>();
            Assert.IsNotNull(report);
            Assert.IsTrue(report.Entries.Any());

            foreach (var entry in report.Entries)
                Assert.AreEqual(dateToQuery, entry.Date);
        }

        [Test]
        [TestCase(UserType.UserManager, AdminUserName)]
        [TestCase(UserType.Regular, AdminUserName)]
        [TestCase(UserType.Regular, ManagerUserName)]
        public async Task GetReportForInvalidRoles_ShouldGetForbidden(UserType userType, string username)
        {
            SetToken(userType);

            var response = await Client.GetAsync(reportEndpoint.Format(username));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task GetReportForNonExistentUsername_ShouldGetNotFound()
        {
            SetToken(UserType.UserManager);

            var response = await Client.GetAsync(reportEndpoint.Format("testUsername"));

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task GetReportForAnotherRegularUserAsRegularUser_ShouldGetForbidden()
        {
            SetToken(UserType.Regular);
            entryDto.Description = "testing";
            entryDto.Amount = 1000;
            entryDto.Date = DateTime.Today;

            var response = await Client.GetAsync(reportEndpoint.Format(alternateRegularUsername));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        #region Post
        [Test]
        public async Task PostEntryAsAnonymous_ShouldGetUnauthorized()
        {
            RemoveToken();

            var response = await Client.PostAsync(endpointRegular, entryDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase(null, null, null)]
        [TestCase(null, 1000, "DateTime.Today")]
        [TestCase("test", null, "DateTime.Today")]
        [TestCase("test", 1000, null)]
        public async Task PostEntryWithWrongData_ShouldReturnBadRequest(string description, double? amount, string date)
        {
            SetToken(UserType.Administrator);
            entryDto.Description = description;
            entryDto.Amount = amount;
            entryDto.Date = date == null ? null : (DateTime?)DateTime.Today;

            var response = await Client.PostAsync(endpointRegular, entryDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            StringAssert.Contains(ValidationError, await response.Content.ReadAsStringAsync());
        }

        [Test]
        [TestCase(UserType.Administrator, AdminUserName)]
        [TestCase(UserType.Administrator, ManagerUserName)]
        [TestCase(UserType.Administrator, RegularUserName)]
        [TestCase(UserType.UserManager, ManagerUserName)]
        [TestCase(UserType.UserManager, RegularUserName)]
        [TestCase(UserType.Regular, RegularUserName)]
        public async Task PostEntryForValidRoles_ShouldReturnCreated(UserType userType, string username)
        {
            SetToken(userType);
            entryDto.Description = "test";
            entryDto.Amount = 1000;
            entryDto.Date = DateTime.Today;

            //POST
            var response = await Client.PostAsync(endpoint.Format(username), entryDto.ToHttpContent());
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.IsNotNull(response.Headers.Location);

            //GET
            response = await Client.GetAsync(response.Headers.Location);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var entry = await response.Content.Deserialize<EntryDto>();
            Assert.IsNotNull(entry);
            Assert.AreEqual(entryDto.Description, entry.Description);
            Assert.AreEqual(entryDto.Amount, entry.Amount);
            Assert.AreEqual(entryDto.Date, entry.Date);
        }

        [Test]
        public async Task PostEntryWithAmountAsZero_ShouldReturnBadRequest()
        {
            SetToken(UserType.Regular);
            entryDto.Description = "test";
            entryDto.Amount = 0;
            entryDto.Date = DateTime.Today;

            var response = await Client.PostAsync(endpoint.Format(RegularUserName), entryDto.ToHttpContent());
            
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("The Amount cannot be 0.", await response.Content.ReadAsStringAsync());
        }

        [Test]
        [TestCase(UserType.UserManager, AdminUserName)]
        [TestCase(UserType.Regular, AdminUserName)]
        [TestCase(UserType.Regular, ManagerUserName)]
        public async Task PostEntryForInvalidRoles_ShouldReturnForbidden(UserType userType, string username)
        {
            SetToken(userType);
            entryDto.Description = "test";
            entryDto.Amount = 1000;
            entryDto.Date = DateTime.Today;

            var response = await Client.PostAsync(endpoint.Format(username), entryDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task PostEntryForNonExistentUsername_ShouldGetNotFound()
        {
            SetToken(UserType.UserManager);
            entryDto.Description = "test";
            entryDto.Amount = 1000;
            entryDto.Date = DateTime.Today;

            var response = await Client.PostAsync(endpoint.Format("testUsername"), entryDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task PostEntryForAnotherRegularUserAsRegularUser_ShouldGetForbidden()
        {
            SetToken(UserType.Regular);
            entryDto.Description = "testing";
            entryDto.Amount = 1000;
            entryDto.Date = DateTime.Today;

            var response = await Client.PostAsync(endpoint.Format(alternateRegularUsername), entryDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        #region Put
        [Test]
        public async Task PutEntryAsAnonymous_ShouldGetUnauthorized()
        {
            RemoveToken();

            var response = await Client.PutAsync(endpointRegular + int.MaxValue, entryDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase(null, null, null)]
        [TestCase(null, 1000, "DateTime.Today")]
        [TestCase("test", null, "DateTime.Today")]
        [TestCase("test", 1000, null)]
        public async Task PutEntryWithWrongData_ShouldReturnBadRequest(string description, double? amount, string date)
        {
            SetToken(UserType.Administrator);
            entryDto.Description = description;
            entryDto.Amount = amount;
            entryDto.Date = date == null ? null : (DateTime?)DateTime.Today;

            var response = await Client.PutAsync(endpointRegular + int.MaxValue, entryDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.IsNotNull(response.Content);
            StringAssert.Contains(ValidationError, await response.Content.ReadAsStringAsync());
        }

        [Test]
        [TestCase(UserType.Administrator, AdminUserName)]
        [TestCase(UserType.Administrator, ManagerUserName)]
        [TestCase(UserType.Administrator, RegularUserName)]
        [TestCase(UserType.UserManager, ManagerUserName)]
        [TestCase(UserType.UserManager, RegularUserName)]
        [TestCase(UserType.Regular, RegularUserName)]
        public async Task PutEntryForValidRoles_ShouldReturnOk(UserType userType, string username)
        {
            SetToken(userType);
            string url = endpoint.Format(username) + data["PUT"][userType][username];
            entryDto.Description = Guid.NewGuid().ToString();
            entryDto.Amount =  1000;
            entryDto.Date = DateTime.Today.AddDays(-1);

            //POST
            var response = await Client.PutAsync(url, entryDto.ToHttpContent());
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            //GET
            response = await Client.GetAsync(url);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsNotNull(response.Content);

            var entry = await response.Content.Deserialize<EntryDto>();
            Assert.IsNotNull(entry);
            Assert.AreEqual(entryDto.Description, entry.Description);
            Assert.AreEqual(entryDto.Amount, entry.Amount);
            Assert.AreEqual(entryDto.Date, entry.Date);
        }

        [Test]
        public async Task PutEntryWithAmountAsZero_ShouldReturnBadRequest()
        {
            SetToken(UserType.Regular);
            entryDto.Description = "test";
            entryDto.Amount = 0;
            entryDto.Date = DateTime.Today;

            var response = await Client.PutAsync(endpointRegular + int.MaxValue, entryDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("The Amount cannot be 0.", await response.Content.ReadAsStringAsync());
        }

        [Test]
        [TestCase(UserType.UserManager, AdminUserName)]
        [TestCase(UserType.Regular, AdminUserName)]
        [TestCase(UserType.Regular, ManagerUserName)]
        public async Task PutEntryForInvalidRoles_ShouldReturnForbidden(UserType userType, string username)
        {
            SetToken(userType);
            entryDto.Description = "test";
            entryDto.Amount = 1000;
            entryDto.Date = DateTime.Today;

            var response = await Client.PutAsync(endpoint.Format(username) + int.MaxValue, entryDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task PutEntryForNonExistentUsername_ShouldGetNotFound()
        {
            SetToken(UserType.UserManager);
            entryDto.Description = "test";
            entryDto.Amount = 1000;
            entryDto.Date = DateTime.Today;

            var response = await Client.PutAsync(endpoint.Format("testUsername") + int.MaxValue, entryDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task PutEntryForAnotherRegularUserAsRegularUser_ShouldGetForbidden()
        {
            SetToken(UserType.Regular);
            entryDto.Description = "testing";
            entryDto.Amount = 1000;
            entryDto.Date = DateTime.Today;

            var response = await Client.PutAsync(endpoint.Format(alternateRegularUsername) + int.MaxValue, entryDto.ToHttpContent());

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        #region Delete
        [Test]
        public async Task DeleteEntryAsAnonymous_ShouldGetUnauthorized()
        {
            RemoveToken();

            var response = await Client.DeleteAsync(endpointRegular + int.MaxValue);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        [TestCase(UserType.Administrator, AdminUserName)]
        [TestCase(UserType.Administrator, ManagerUserName)]
        [TestCase(UserType.Administrator, RegularUserName)]
        [TestCase(UserType.UserManager, ManagerUserName)]
        [TestCase(UserType.UserManager, RegularUserName)]
        [TestCase(UserType.Regular, RegularUserName)]
        public async Task DeleteEntryForValidRoles_ShouldDeleteEntry(UserType userType, string username)
        {
            SetToken(UserType.Administrator);
            int entryId = data["DELETE"][userType][username];
            string url = endpoint.Format(username) + entryId;

            //DELETE
            var response = await Client.DeleteAsync(url);
            Assert.IsTrue(response.IsSuccessStatusCode);

            //GET
            response = await Client.GetAsync(url);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        [TestCase(UserType.UserManager, AdminUserName)]
        [TestCase(UserType.Regular, AdminUserName)]
        [TestCase(UserType.Regular, ManagerUserName)]
        public async Task DeleteEntryForInvalidRoles_ShouldGetForbidden(UserType userType, string username)
        {
            SetToken(userType);

            var response = await Client.GetAsync(endpoint.Format(username) + int.MaxValue);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        [TestCase(UserType.Administrator, AdminUserName)]
        [TestCase(UserType.UserManager, ManagerUserName)]
        [TestCase(UserType.Regular, RegularUserName)]
        public async Task DeleteNonExistentEntry_ShouldGetNotFound(UserType userType, string username)
        {
            SetToken(userType);

            var response = await Client.GetAsync(endpoint.Format(username) + int.MaxValue);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task DeleteEntryForNonExistentUsername_ShouldGetNotFound()
        {
            SetToken(UserType.UserManager);

            var response = await Client.DeleteAsync(endpoint.Format("testUsername") + int.MaxValue);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task DeleteEntryForAnotherRegularUserAsRegularUser_ShouldGetForbidden()
        {
            SetToken(UserType.Regular);

            var response = await Client.DeleteAsync(endpoint.Format(alternateRegularUsername) + int.MaxValue);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion
    }
}
