using Newtonsoft.Json.Linq;

namespace Module.Services.OccupanyModels
{
    public class OccupancyDevice
    {
        public OccupancyDevice(string deviceEUI, string metadata)
        {
            DevEUI = deviceEUI;

            var metadataJObj = JObject.Parse(metadata);
            var json = ((JValue)metadataJObj["OccupancyPerHour"])?.Value<string>();
            if (json != null)
            {
                var jobj = JObject.Parse(json);
                DevEUIColumn = ((JValue)jobj["deveui"])?.Value<string>();
                TimeColumn = ((JValue)jobj["time"])?.Value<string>();
                OccupancyColumn = ((JValue)jobj["occupancy"])?.Value<string>();
                UseUTCTime = ((JValue)jobj["useUTCTime"])?.Value<bool>() ?? false;
            }
            IsValid = json != null && DevEUIColumn != null && TimeColumn != null && OccupancyColumn != null;
        }

        public string DevEUI { get; set; }
        
        public string DevEUIColumn { get; set; }

        /// <summary>If data is added correct</summary>
        public bool IsValid { get; set; }
        
        public string OccupancyColumn { get; set; }
        
        public string TimeColumn { get; set; }
        
        public bool UseUTCTime { get; set; }

    }
}
