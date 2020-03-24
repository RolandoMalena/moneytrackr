using static MoneyTrackr.Constants.User;

namespace MoneyTrackr.Helpers
{
    public static class UserHelper
    {
        public static string GetUserId(string username)
        {
            switch (username.ToUpper())
            {
                case NormalizedAdminUserName: return AdminUserId;
                case NormalizedManagerUserName: return ManagerUserId;
                case NormalizedRegularUserName: return RegularUserId;
                default: return null;
            }
        }
    }
}
