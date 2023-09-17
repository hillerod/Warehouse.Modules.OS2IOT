using Module.Services.Models.OS2IOT;
using Newtonsoft.Json.Linq;
using System;

namespace Module.Services.Models.Mssql
{
    public class MssqlDevice
    {
        private string _whTableName;
        private string _whDevEuiProperty;
        private string _whOccupancyProperty;
        private string _whOccupancyTableName;
        private string _whTimeProperty;

        public MssqlDevice() { }

        public MssqlDevice(Device iotDevice)
        {
            if (iotDevice == null)
                return;

            Id = iotDevice.id;
            ApplicationId = iotDevice.application.id;
            DeviceModelId = iotDevice.deviceModel?.id;
            ChirpstackApplicationId = iotDevice.chirpstackApplicationId;
            Name = iotDevice.name;
            DeviceEUI = iotDevice.deviceEUI;
            Comment = iotDevice.comment;
            CommentOnLocation = iotDevice.commentOnLocation;
            Metadata = iotDevice.metadata.ToString();
            Latitude = iotDevice.location.coordinates[0];
            Longitude = iotDevice.location.coordinates[1];
            LastRecievedMessageTime = iotDevice.latestReceivedMessage?.sentTime;
            LoraActivationType = iotDevice.lorawanSettings?.activationType;
            LoraBatteryStatus = iotDevice.lorawanSettings?.deviceStatusBattery;
            CreatedAt = iotDevice.createdAt;
            UpdatedAt = iotDevice.updatedAt;
            CreatedById = iotDevice.createdBy;
            UpdatedById = iotDevice.updatedBy;
        }

        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int? DeviceModelId { get; set; }
        public int ChirpstackApplicationId { get; set; }
        public string Name { get; set; }
        public string DeviceEUI { get; set; }
        public string Comment { get; set; }
        public string CommentOnLocation { get; set; }
        public string Metadata { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public DateTime? LastRecievedMessageTime { get; set; }
        public string LoraActivationType { get; set; }
        public int? LoraBatteryStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CreatedById { get; set; }
        public int UpdatedById { get; set; }

        public string WHTableName => _whTableName ??= JToken.Parse(Metadata)["WHTableName"]?.Value<string>();
        public string WHDevEuiProperty => _whDevEuiProperty ??= JToken.Parse(Metadata)["WHDevEuiProperty"]?.Value<string>();
        public string WHOccupancyProperty => _whOccupancyProperty ??= JToken.Parse(Metadata)["WHOccupancyProperty"]?.Value<string>();
        public string WHOccupancyTableName => _whOccupancyTableName ??= JToken.Parse(Metadata)["WHOccupancyTableName"]?.Value<string>();
        public string WHTimeProperty => _whTimeProperty ??= JToken.Parse(Metadata)["WHTimeProperty"]?.Value<string>();
    }
}
