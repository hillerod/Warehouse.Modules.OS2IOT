using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Module.Services.Models.Mssql
{
    public class MssqlDevices
    {
		private IEnumerable<string> _devEUINames;

        public MssqlDevices(MssqlDevice[] iotDevices) => IOTDevices = iotDevices;

        public IEnumerable<MssqlDevice> IOTDevices { get; }

        [JsonIgnore]
        public IEnumerable<string> DevEUINames => _devEUINames ??= IOTDevices.Select(o=> o.WHDevEuiProperty)?.Where(o=> o != null)?.Distinct();

    }
}
