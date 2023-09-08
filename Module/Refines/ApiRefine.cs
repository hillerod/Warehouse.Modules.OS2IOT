using Bygdrift.Tools.CsvTool;
using Bygdrift.Tools.DataLakeTool;
using Bygdrift.Warehouse;
using Module.Services.OS2IOTModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// A refine file, refines incomming data and save it to the public property 'Csv'
/// Afterwards, it describes, how to save the csv to datalake and/or database
/// </summary>
namespace Module.Refines
{
    public class ApiRefine
    {
        private static AppBase<Settings> App;
        public static async Task RefineAsync(AppBase<Settings> app, Organizations organizations, Applications applications, DeviceModels deviceModels, IEnumerable<IotDevice> iotDevices, ChirpstackGateway[] gateways)
        {
            App = app;
            await Add("Organizations", organizations, CreateOrganizationsCsv(organizations), "id");
            await Add("ChirpstackGateways", gateways, CreateGatewaysCsv(gateways), "id");
            await Add("Applications", applications, CreateApplicationsCsv(applications), "id");
            await Add("DeviceModels", deviceModels, CreateDeviceModelsCsv(deviceModels), "id");
            await Add("IotDevices", iotDevices, CreateIotDevicesCsv(iotDevices), "id");
        }

        private static async Task Add(string name, object data, Csv csv, string primaryKeyId) 
        {
            App.Log.LogInformation($"Refine and save {name}...");
            await App.DataLake.SaveObjectAsync(data, "ApiRaw", name + ".json", FolderStructure.DatePath);
            await App.DataLake.SaveCsvAsync(csv, "ApiRefined", name + ".csv", FolderStructure.DatePath);
            App.Mssql.MergeCsv(csv, name, primaryKeyId, false, false);
        }

        private static Csv CreateGatewaysCsv(ChirpstackGateway[] gateways)
        {
            var csv = new Csv();
            foreach (var item in gateways)
                csv.AddObject(item, true);

            return csv;
        }

        private static Csv CreateOrganizationsCsv(Organizations organizations)
        {
            var csv = new Csv("id, name, applicationIds, createdAt, updatedAt");
            foreach (var a in organizations.data)
            {
                var applicationIds = a.applications != null ? string.Join(',', a.applications.Select(p => p.id)) : null;
                csv.AddRow(a.id, a.name, applicationIds, a.createdAt, a.updatedAt);
            }

            return csv;
        }

        private static Csv CreateApplicationsCsv(Applications applications)
        {
            var csv = new Csv("id, name, description, createdAt, updatedAt");
            foreach (var a in applications.data)
                csv.AddRow(a.id, a.name, a.description, a.createdAt, a.updatedAt);

            return csv;
        }

        private static Csv CreateDeviceModelsCsv(DeviceModels deviceModels)
        {
            var csv = new Csv("id, name, type, category, brandName, modelName, manufacturerName, controlledProperties, createdAt, updatedAt");
            foreach (var d in deviceModels.data)
            {
                var controlledProperties = d.body.controlledProperty.Any() ? string.Join(',', d.body.controlledProperty) : null;
                csv.AddRow(d.id, d.body.name, d.body.type, d.body.category, d.body.brandName, d.body.modelName, d.body.manufacturerName, controlledProperties, d.createdAt, d.updatedAt);
            }

            return csv;
        }

        private static Csv CreateIotDevicesCsv(IEnumerable<IotDevice> iotDevices)
        {
            var csv = new Csv("id, applicationId, deviceModelId, chirpstackApplicationId, name, deviceEUI, comment, commentOnLocation, metadata, latitude, longitude, lastRecievedMessageTime, loraActivationType, loraBatteryStatus, createdAt, updatedAt, createdById, updatedById");
            foreach (var i in iotDevices)
            {
                var lat = i.location.coordinates[0];
                var lon = i.location.coordinates?[1];
                csv.AddRow(i.id, i.application.id, i.deviceModel.id, i.chirpstackApplicationId, i.name, i.deviceEUI, i.comment, i.commentOnLocation, i.metadata, 
                    lat, lon, i.latestReceivedMessage.sentTime, i.lorawanSettings.activationType, i.lorawanSettings.deviceStatusBattery, i.createdAt, i.updatedAt, i.createdBy, i.updatedBy);
            }

            return csv;
        }
    }
}