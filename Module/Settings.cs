using Bygdrift.Warehouse.Helpers.Attributes;

namespace Module
{
    public class Settings
    {
        [ConfigSetting]
        public string OS2IOTApiBaseUrl { get; set; }
        
        [ConfigSetting]
        public int MonthsToKeepDataInDataLake { get; set; }

        [ConfigSecret]
        public string UserName { get; set; }

        [ConfigSecret]
        public string Password { get; set; }

        [ConfigSecret]
        public string MethodPostAuthorization { get; set; }
    }
}
