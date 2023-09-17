using System;

namespace Module.Services.Models.OS2IOT
{

    public class Devicemodels
    {
        public DevicemodelDatum[] data { get; set; }
        public int count { get; set; }
    }

    public class DevicemodelDatum
    {
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public DevicemodelBody body { get; set; }
    }

    public class DevicemodelBody
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string category { get; set; }
        public string[] function { get; set; }
        public string brandName { get; set; }
        public string modelName { get; set; }
        public string[] supportedUnits { get; set; }
        public string manufacturerName { get; set; }
        public string[] supportedProtocol { get; set; }
        public string[] controlledProperty { get; set; }
        public string energyLimitationClass { get; set; }
    }
}
