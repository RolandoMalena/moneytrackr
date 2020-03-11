using static MoneyTrackr.Constants.Role;

namespace MoneyTrackr.Helpers
{
    public static class RoleHelper
    {
        public static string GetRoleId(string roleName)
        {
            switch (roleName.ToUpper())
            {
                case NormalizedAdministratorRoleName: return AdministratorRoleId;
                case NormalizedUserManagerRoleName: return UserManagerRoleId;
                case NormalizedRegularUserRoleName: return RegularUserRoleId;
                default: return null;
            }
        }

        public static string GetRoleName(string roleId, bool normalized = false)
        {
            string role = null;

            switch (roleId)
            {
                case AdministratorRoleId: role = AdministratorRoleName; break;
                case UserManagerRoleId: role = UserManagerRoleName; break;
                case RegularUserRoleId: role = RegularUserRoleName; break;
                default: return null;
            }

            if (normalized)
                role = role.ToUpper();

            return role;
        }
    }
}
