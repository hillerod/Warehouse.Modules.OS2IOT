using System;

namespace Module.Services.OS2IOTModels
{

    public class ChirpstackGateways
    {
        public ChirpstackGateway[] result { get; set; }
        public int totalCount { get; set; }
    }

    public class ChirpstackGateway
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public object firstSeenAt { get; set; }
        public object lastSeenAt { get; set; }
        public string organizationID { get; set; }
        public string networkServerID { get; set; }
        public GatewayLocation location { get; set; }
        public string networkServerName { get; set; }
        public int internalOrganizationId { get; set; }
    }

    public class GatewayLocation
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
        public int altitude { get; set; }
        public string source { get; set; }
        public int accuracy { get; set; }
    }

}
