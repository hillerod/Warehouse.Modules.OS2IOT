using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.Services.OS2IOTModels
{

    public class IotDevice
    {
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string name { get; set; }
        public IotDeviceLocation location { get; set; }
        public string commentOnLocation { get; set; }
        public string comment { get; set; }
        public object metadata { get; set; }
        public string type { get; set; }
        public string deviceEUI { get; set; }
        public int chirpstackApplicationId { get; set; }
        public IotDeviceApplication application { get; set; }
        public Receivedmessagesmetadata[] receivedMessagesMetadata { get; set; }
        public Latestreceivedmessage latestReceivedMessage { get; set; }
        public Devicemodel deviceModel { get; set; }
        public int createdBy { get; set; }
        public int updatedBy { get; set; }
        public Lorawansettings lorawanSettings { get; set; }
    }

    public class IotDeviceLocation
    {
        public string type { get; set; }
        public float[] coordinates { get; set; }
    }

    public class IotDeviceApplication
    {
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class Latestreceivedmessage
    {
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public Rawdata rawData { get; set; }
        public DateTime sentTime { get; set; }
    }

    public class Rawdata
    {
        public bool adr { get; set; }
        public string data { get; set; }
        public int fCnt { get; set; }
        public Tags tags { get; set; }
        public int fPort { get; set; }
        public string devEUI { get; set; }
        public Rxinfo[] rxInfo { get; set; }
        public Txinfo txInfo { get; set; }
        public string deviceName { get; set; }
        public string applicationID { get; set; }
        public string applicationName { get; set; }
        public string deviceProfileID { get; set; }
        public string deviceProfileName { get; set; }
    }

    public class Tags
    {
        public string os2iotcreatedby { get; set; }
        public string os2iotupdatedby { get; set; }
        public string internalOrganizationId { get; set; }
    }

    public class Txinfo
    {
        public int dr { get; set; }
        public int frequency { get; set; }
    }

    public class Rxinfo
    {
        public string name { get; set; }
        public int rssi { get; set; }
        public float loRaSNR { get; set; }
        public Location1 location { get; set; }
        public string uplinkID { get; set; }
        public string gatewayID { get; set; }
    }

    public class Location1
    {
        public int altitude { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
    }

    public class Devicemodel
    {
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public Body body { get; set; }
    }

    public class Body
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

    public class Lorawansettings
    {
        public string activationType { get; set; }
        public string OTAAapplicationKey { get; set; }
        public string devEUI { get; set; }
        public string deviceProfileID { get; set; }
        public string serviceProfileID { get; set; }
        public bool skipFCntCheck { get; set; }
        public bool isDisabled { get; set; }
        public int deviceStatusBattery { get; set; }
        public int deviceStatusMargin { get; set; }
    }

    public class Receivedmessagesmetadata
    {
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public DateTime sentTime { get; set; }
        public object signalData { get; set; }
    }

}
