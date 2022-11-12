using System;

namespace Module.Services.OS2IOTModels
{

    public class DeviceModels
    {
        public DeviceModelDatum[] data { get; set; }
        public int count { get; set; }
    }

    public class DeviceModelDatum
    {
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public DeviceModelBody body { get; set; }
    }

    public class DeviceModelBody
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string category { get; set; }
        public string brandName { get; set; }
        public string modelName { get; set; }
        public string manufacturerName { get; set; }
        public string[] controlledProperty { get; set; }
    }

}
