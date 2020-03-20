using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using static MoneyTrackr.Constants.Role;
using static MoneyTrackr.Constants.User;

namespace MoneyTrackr.Tests
{
    [SetUpFixture]
    public class TestBase : MoneyTrackrWebApplicationFactory<Startup>
    {
        public HttpClient Client { get; set; }

        private Dictionary<UserType, string> JWTs = new Dictionary<UserType, string>();

        #region SetUp and TearDown
        [OneTimeSetUp]
        public void Setup()
        {
            Client = Server.CreateClient();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Client.Dispose();
            Server.Dispose();
        }
        #endregion

        #region Helpers used in inherited classes
        /// <summary>
        /// Sets a new JWT to authenticate via HTTP to the Authorization Header of the Client object
        /// </summary>
        /// <param name="userType">The UserType you want to authenticate as</param>
        public void SetToken(UserType userType)
        {
            //If we already have generated a token for this UserType, use it and return
            if (JWTs.ContainsKey(userType))
            {
                Client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", JWTs[userType]);

                return;
            }

            string userName;
            string roleName;

            //Start by getting the corresponding UserName and Role
            switch (userType)
            {
                case UserType.Administrator:
                    {
                        userName = AdminUserName;
                        roleName = AdministratorRoleName;
                    }
                    break;
                case UserType.UserManager:
                    {
                        userName = ManagerUserName;
                        roleName = UserManagerRoleName;
                    }
                    break;
                case UserType.Regular:
                    {
                        userName = RegularUserName;
                        roleName = RegularUserRoleName;
                    }
                    break;

                default:
                    throw new NotImplementedException($"UserType {userType} has not been implemented.");
            }

            //Set the claims
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userName),
                new Claim(ClaimTypes.Role, roleName)
            };

            //Generate the JWT based on the claims
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Configuration["Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwt = tokenHandler.WriteToken(token);

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            //Add the Token to the Dictionary
            JWTs.Add(userType, jwt);
        }

        public void RemoveToken()
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
        #endregion
    }

    public enum UserType
    {
        Regular,
        UserManager,
        Administrator
    }
}
