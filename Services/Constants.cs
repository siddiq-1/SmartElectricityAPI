namespace SmartElectricityAPI.Services;

public static class Constants
{
    public static string CompanyId = "CompanyId";
    public static int AccessTokenExpireInMinutes = 20;
    public static int RefreshTokenExpireInMinutes = 60 * 24 * 7; // 7 days currently
    public static int CheckInverterManualButtonsStateEveryXminute = 1;
    public static int CheckSofarStateBufferStateEveryXminute = 1;
    public static int RestartSystemIntervalForBrokerService = 30;
    public static int SofarStateMinutesPeriod = 4;
    public static string UserPermissionId = "UserPermissionId";
    public static string UserPermission = "UserPermission";
    public static string IsAdmin = "IsAdmin";
    public static string SelectedCompanyId = "SelectedCompanyId";
    public static string UserId = "UserId";
    public static int NumberOfPricesToGet = 33;
    public static double MqttLogUnixOffset  => DateTimeOffset.UtcNow.AddMinutes(-120).ToUnixTimeSeconds();
    public static double SofarStateUnixOffset => DateTimeOffset.UtcNow.AddMinutes(-120).ToUnixTimeSeconds();


    public static class CacheKeys
    {
        public static string DbPermissions = "DbPermissions";
        public static Func<int, string> MqttLogKey = inverterId => $"MqttLogKey{inverterId}";
    }

    public static class Tokens
    {
        public static string AccessTokenSecret = "!Sometsdfdfdffffffff!Rffdfffffffffffffffffdfffffff45454rffffsdfsdfdhingSecredfdfdfdfefdt!";
        public static string RefreshTokenSecret = "!Sometsdfdfdffffffff!Rffdfffffffffffffffffdfffffff45454rffffsdfsdfdhingSecredfdfdfd123dt!";
    }
}
