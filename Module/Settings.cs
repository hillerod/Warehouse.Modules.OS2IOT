using Bygdrift.Warehouse.Attributes;

namespace Module
{
    public class Settings
    {
        [ConfigSetting]
        public string OS2IOTApiBaseUrl { get; set; }

        [ConfigSetting]
        public int MonthsToKeepDataInDataLake { get; set; }

        [ConfigSecret]
        public string OS2IOTApiUserName { get; set; }

        [ConfigSecret]
        public string OS2IOTApiPassword { get; set; }

        [ConfigSecret]
        public string PostPayloadsAuthorizationKey { get; set; }
    }
}
