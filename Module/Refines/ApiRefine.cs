using Bygdrift.Tools.CsvTool;
using Bygdrift.Tools.DataLakeTool;
using Bygdrift.Warehouse;
using Module.Services.Models.Mssql;
using Module.Services.Models.OS2IOT;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Module.Refines
{
    public class ApiRefine
    {
        private static AppBase<Settings> App;
        private static bool AddToDataLakeAnsMssql;
        public static async Task RefineAsync(AppBase<Settings> app, bool addToDataLakeAnsMssql, Organizations organizations, Applications applications, Devicemodels deviceModels, MssqlDevices iotDevices, ChirpstackGateway[] gateways)
        {
            App = app;
            AddToDataLakeAnsMssql = addToDataLakeAnsMssql;
            await Add("Organizations", organizations, CreateOrganizationsCsv(organizations), "id");
            await Add("ChirpstackGateways", gateways, CreateGatewaysCsv(gateways), "id");
            await Add("Applications", applications, CreateApplicationsCsv(applications), "id");
            await Add("DeviceModels", deviceModels, CreateDeviceModelsCsv(deviceModels), "id");
            await Add("IotDevices", iotDevices, CreateIotDevicesCsv(iotDevices), "Id");
        }

        private static async Task Add(string name, object data, Csv csv, string primaryKeyId)
        {
            if (data == null)
                return;

            App.Log.LogInformation($"Refine and save {name}...");
            if (AddToDataLakeAnsMssql)
            {
                await App.DataLake.SaveObjectAsync(data, "ApiRaw", name + ".json", FolderStructure.DatePath);
                await App.DataLake.SaveCsvAsync(csv, "ApiRefined", name + ".csv", FolderStructure.DatePath);
                App.Mssql.MergeCsv(csv, name, primaryKeyId, false, false);
            }
        }

        private static Csv CreateOrganizationsCsv(Organizations organizations)
        {
            if (organizations == null)
                return null;

            var csv = new Csv("id, name, applicationIds, createdAt, updatedAt");
            foreach (var a in organizations.data)
            {
                var applicationIds = a.applications != null ? string.Join(',', a.applications.Select(p => p.id)) : null;
                csv.AddRow(a.id, a.name, applicationIds, a.createdAt, a.updatedAt);
            }

            return csv;
        }

        private static Csv CreateGatewaysCsv(ChirpstackGateway[] gateways)
        {
            if (gateways == null)
                return null;

            var csv = new Csv();
            foreach (var item in gateways)
                csv.AddObject(item, true);

            return csv;
        }

        private static Csv CreateApplicationsCsv(Applications applications)
        {
            var csv = new Csv("id, name, description, createdAt, updatedAt, status, startDate, endDate, category, owner, contactPerson, constactEmail, contactPhone, personalData, hardware");
            foreach (var a in applications.data)
                csv.AddRow(a.id, a.name, a.description, a.createdAt, a.updatedAt, a.status, a.startDate, a.endDate, a.category, a.owner, a.contactPerson, a.contactEmail, a.contactPhone, a.personalData, a.hardware);

            return csv;
        }

        private static Csv CreateDeviceModelsCsv(Devicemodels deviceModels)
        {
            var csv = new Csv("id, name, type, category, brandName, modelName, manufacturerName, controlledProperties, createdAt, updatedAt");
            foreach (var d in deviceModels.data)
            {
                var controlledProperties = d.body.controlledProperty.Any() ? string.Join(',', d.body.controlledProperty) : null;
                csv.AddRow(d.id, d.body.name, d.body.type, d.body.category, d.body.brandName, d.body.modelName, d.body.manufacturerName, controlledProperties, d.createdAt, d.updatedAt);
            }

            return csv;
        }

        private static Csv CreateIotDevicesCsv(MssqlDevices iotDevices)
        {
            return new Csv().AddModel(iotDevices.IOTDevices);
        }
    }
}