using static MoneyTrackr.Constants.Role;

namespace MoneyTrackr.Helpers
{
    public static class RoleHelper
    {
        public static string GetRoleName(string roleId)
        {
            switch (roleId)
            {
                case AdministratorRoleId: return AdministratorRoleName;
                case UserManagerRoleId: return UserManagerRoleName;
                case RegularUserRoleId: return RegularUserRoleName;
                default: return null;
            }
        }
    }
}
