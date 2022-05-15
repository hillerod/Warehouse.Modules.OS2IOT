using Bygdrift.CsvTools;
using Bygdrift.DataLakeTools;
using Bygdrift.Warehouse;
using Module.Services.Models;
using System;
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
        public static async Task Refine(AppBase<Settings> app, Applications applications, DeviceModels deviceModels, IEnumerable<IotDevice> iotDevices)
        {
            app.Log.LogInformation("Refining data...");

            await app.DataLake.SaveObjectAsync(applications, "ApiRaw", "Applications.json", FolderStructure.DatePath);
            await app.DataLake.SaveObjectAsync(deviceModels, "ApiRaw", "DeviceModels.json", FolderStructure.DatePath);

            app.Log.LogInformation("Refine and save Applications...");
            var appCsv = CreateApplicationsCsv(applications);
            await app.DataLake.SaveCsvAsync(appCsv, "ApiRefined", "Applications.csv", FolderStructure.DatePath);
            app.Mssql.InserCsv(appCsv, "Applications", false, false);


            app.Log.LogInformation("Refine and save DeviceModels...");
            var modelCsv = CreateDeviceModelsCsv(deviceModels);
            await app.DataLake.SaveCsvAsync(appCsv, "ApiRefined", "DeviceModels.csv", FolderStructure.DatePath);
            app.Mssql.InserCsv(modelCsv, "DeviceModels", false, false);

            app.Log.LogInformation("Refine and save IOTDevices...");
            var iotCsv = CreateIotDevicesCsv(iotDevices);
            await app.DataLake.SaveCsvAsync(appCsv, "ApiRefined", "IotDevices.csv", FolderStructure.DatePath);
            app.Mssql.InserCsv(iotCsv, "IotDevices", false, false);
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