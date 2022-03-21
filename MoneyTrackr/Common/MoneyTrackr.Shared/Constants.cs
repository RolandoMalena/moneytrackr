namespace MoneyTrackr.Shared
{
    public static class Constants
    {
        public static class ClaimTypes
        {
            public const string FinanceManagementLevel = "FinanceManagementLevel";
            public const string UserManagementLevel = "UserManagementLevel";
        }

        public static class FinanceManagementLevels
        {
            public const string Self = "Self";
            public const string SelfAndRegularUsers = "SelfAndRegularUsers";
            public const string AllUsers = "AllUsers";
        }

        public static class UserManagementLevels
        {
            public const string None = "None";
            public const string RegularUsers = "RegularUsers";
            public const string AllUsers = "AllUsers";
        }
    }
}
