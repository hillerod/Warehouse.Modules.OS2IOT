using System;

namespace Module.Services.Models.OS2IOT
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
        public string status { get; set; }
        public object startDate { get; set; }
        public object endDate { get; set; }
        public string category { get; set; }
        public string owner { get; set; }
        public string contactPerson { get; set; }
        public string contactEmail { get; set; }
        public string contactPhone { get; set; }
        public object personalData { get; set; }
        public string hardware { get; set; }
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
