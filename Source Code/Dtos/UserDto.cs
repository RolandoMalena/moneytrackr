using Microsoft.AspNetCore.Identity;
using MoneyTrackr.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static MoneyTrackr.Constants.Role;

namespace MoneyTrackr.Dtos
{
    #region UserLogInDto
    public class UserLogInDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
    #endregion

    #region UserDto
    public class UserDto
    {
        public virtual string Id { get; set; }

        //Marked as not required to allow updating without the Username attached to it.
        [StringLength(25, MinimumLength = 5, ErrorMessage = "The Username must be between {2} and {1} characters long.")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        //Marked as not required to allow updating without the password attached to it.
        [StringLength(25, MinimumLength = 5, ErrorMessage = "The Password must be between {2} and {1} characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public virtual string RoleId { get; set; }
        public virtual RoleDto Role { get; set; }

        /// <summary>
        /// Converts the Dto to the Model Object. It also sets the hashed password and the RoleId
        /// </summary>
        /// <param name="hashedPassword">The hashed password to be set.</param>
        public IdentityUser Convert(string hashedPassword)
        {
            var user = new IdentityUser()
            {
                UserName = UserName,
                PasswordHash = hashedPassword
            };

            return user;
        }

        /// <summary>
        /// Converts a Model Object into a new instance of a Dto, optionally with the RoleId
        /// </summary>
        public static UserDto ConvertBack(IdentityUser user, string roleId = null)
        {
            var dto = new UserDto()
            {
                Id = user.Id,
                UserName = user.UserName
            };

            if (RoleHelper.GetRoleName(roleId) != null)
                dto.Role = new RoleDto(roleId);

            return dto;
        }
    }
    #endregion

    #region RegisterUserDto
    public class RegisterUserDto : UserDto
    {
        public override string Id
        {
            get { return null; }
            set { }
        }

        public override string RoleId
        {
            get { return RegularUserRoleId; }
            set { }
        }

        public override RoleDto Role
        {
            get { return null; }
            set { }
        }
    }
    #endregion

    #region RoleDto
    public class RoleDto
    {
        public RoleDto(string roleId)
        {
            Id = roleId;
            Name = RoleHelper.GetRoleName(roleId);
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
    #endregion
}
