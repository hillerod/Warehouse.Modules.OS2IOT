using Bygdrift.Warehouse;
using Module.Services.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

//Documentation: https://backend.os2iot.gate21.dk/api/v1/docs/#/
namespace Module.Services
{
    public class ApiService
    {
        private HttpClient _client;
        public string token;
        private DateTime tokenFetched;

        public AppBase<Settings> App { get; }

        public ApiService(AppBase<Settings> app) => App = app;

        public HttpClient Client
        {
            get
            {
                if (_client == null || token == null || DateTime.Now.Subtract(tokenFetched).TotalHours > 8)
                    _client = GetHttpClient();

                return _client;
            }
        }

        public async Task<AuthProfile> GetAuthProfileAsync()
        {
            return await GetAsync<AuthProfile>("/api/v1/auth/profile");
        }

        public async Task<DeviceModels> GetDeviceModelsAsync()
        {
            return await GetAsync<DeviceModels>("/api/v1/device-model");
        }

        public async Task<Applications> GetApplicationsAsync()
        {
            return await GetAsync<Applications>("/api/v1/Application");
        }

        public async Task<IotDevice> GetIOTDeviceAsync(int deviceId)
        {
            return await GetAsync<IotDevice>("/api/v1/iot-device/" + deviceId);
        }

        public async Task<IotDevice[]> GetIOTDevicesAsync(Applications applications)
        {
            var tasks = new List<Task<IotDevice>>();
            var res = new List<IotDevice>();
            var ids = applications.data.SelectMany(o => o.iotDevices).Select(p=> p.id);

            foreach (var item in ids)
                tasks.Add(GetIOTDeviceAsync(item));

            await Task.WhenAll(tasks);
            return tasks.Select(o => o.Result).ToArray();
        }

        public async Task<Organizations> GetOrganizationsAsync()
        {
            return await GetAsync<Organizations>("/api/v1/organization");
        }

        public async Task<ChirpstackGateway[]> GetChirpstackGatewaysAsync(int organizationId)
        {
            return (await GetAsync<ChirpstackGateways>($"/api/v1/chirpstack/gateway?organizationId={organizationId}")).result;
        }

        public async Task<ChirpstackGateway[]> GetChirpstackGatewaysAsync(Organizations organizations)
        {
            var tasks = new List<Task<ChirpstackGateway[]>>();
            var res = new List<ChirpstackGateway>();
            var ids = organizations.data.Select(o => o.id);

            foreach (var item in ids)
                tasks.Add(GetChirpstackGatewaysAsync(item));

            await Task.WhenAll(tasks);
            return tasks.SelectMany(o => o.Result).ToArray();
        }


        public async Task<T> GetAsync<T>(string subUrl)
        {
            var delimiter = subUrl.Contains('?') ? "&" : "?";
            var response = await Client.GetAsync($"{subUrl}{delimiter}limit=1000");
            if (response.StatusCode != HttpStatusCode.OK)
            {
                App.Log.LogError("The webservice {Url} failed. Error: {Error}.", response.RequestMessage.RequestUri, response.ReasonPhrase);
                return default;
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        private HttpClient GetHttpClient()
        {
            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);
            client.Timeout = new TimeSpan(1, 0, 0);
            client.BaseAddress = new Uri(App.Settings.OS2IOTApiBaseUrl);

            var user = "{\"username\":\"" + App.Settings.UserName + "\", \"password\":\"" + App.Settings.Password + "\"}";
            var data = new StringContent(user, Encoding.UTF8, "application/json");
            var response = client.PostAsync("/api/v1/auth/login", data).Result;
            if (response.StatusCode != HttpStatusCode.Created)
            {
                App.Log.LogError($"The API failed to fetch jwtToken. Error: {response.ReasonPhrase}.");
                client.Dispose();
            }
            var json = response.Content.ReadAsStringAsync().Result;
            token = ((dynamic)JsonConvert.DeserializeObject(json)).accessToken;

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            tokenFetched = DateTime.Now;
            return client;
        }
    }
}
