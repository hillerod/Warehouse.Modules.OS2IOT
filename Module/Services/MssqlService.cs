using Bygdrift.Warehouse;
using Module.Services.Models.Mssql;
using RepoDb;
using System.Collections.Generic;
using System.Linq;

namespace Module.Services
{
    public class MssqlService
    {
        public AppBase<Settings> App { get; }

        public MssqlService(AppBase<Settings> app) => App = app;

        public MssqlDevice[] GetIotDevices()
        {
            var sql = $"SELECT deviceEUI, metadata FROM [{App.ModuleName}].IotDevices";
            return App.Mssql.Connection.ExecuteQuery<MssqlDevice>(sql).ToArray();
        }
    }
}
