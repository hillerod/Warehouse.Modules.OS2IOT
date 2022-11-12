using System;

namespace Module.Services.OS2IOTModels
{

    public class Applications
    {
        public ApplicationDatum[] data { get; set; }
        public int count { get; set; }
    }

    public class ApplicationDatum
    {
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public ApplicationsIOTdevice[] iotDevices { get; set; }
    }

    public class ApplicationsIOTdevice
    {
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string name { get; set; }
        public Location location { get; set; }
        public string commentOnLocation { get; set; }
        public string comment { get; set; }
        public object metadata { get; set; }
        public string type { get; set; }
        public string deviceEUI { get; set; }
        public int chirpstackApplicationId { get; set; }
    }

    public class Location
    {
        public string type { get; set; }
        public float[] coordinates { get; set; }
    }

}
