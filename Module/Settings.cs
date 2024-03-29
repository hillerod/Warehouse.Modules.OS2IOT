﻿using Bygdrift.Warehouse.Attributes;

namespace Module
{
    public class Settings
    {
        [ConfigSetting]
        public string OS2IOTApiBaseUrl { get; set; }

        [ConfigSetting(Default=10000)]
        public int MonthsToKeepDataInDataLake { get; set; }

        [ConfigSecret]
        public string OS2IOTApiUserName { get; set; }

        [ConfigSecret]
        public string OS2IOTApiPassword { get; set; }

        [ConfigSecret]
        public string OS2IOTAuthorization { get; set; }

        [ConfigSetting(Default = false)]  //Can only be done if user is administrator
        public bool GetOS2IOTApiOrganizationAndGateways { get; set; }

        [ConfigSetting(Default = false)]
        public bool CalculateOccupancyPerHour { get; set; }

        [ConfigSetting(Default = false)]
        public bool IngestQueuedPayloads { get; set; }

        [ConfigSetting]
        public string Owner { get; set; }
    }
}
