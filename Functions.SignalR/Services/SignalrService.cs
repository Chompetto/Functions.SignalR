using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Functions.SignalR.Services
{
    public interface ISignalrService
    {
        Task SendToAll(string hub, string targetMethod, object[] data);
        Task SendToGroup(string hubName, string groupName, string targetMethod, object[] data);
        Task SendToUser(string hubName, string userId, string targetMethod, object[] data);
        Task AddUserToGroup(string hubName, string groupName, string userId);
        Task RemoveUserFromGroup(string hubname, string groupName, string userId);
        SignalrConnectionInfo GetClientConnection(string hubName, string userId = null);
    }
    public class SignalrService : ISignalrService
    {
        private readonly HttpClient _Client;
        private readonly ITokenService _TokenService;
        private readonly IEndPointService _EndPointService;
        private readonly string _Endpoint;
        private readonly string _AccessKey;

        public SignalrService(string endpoint, string accessKey, ITokenService tokenService, IEndPointService endPointService)
        {
            _Endpoint = endpoint;
            _AccessKey = accessKey;
            _EndPointService = endPointService;
            _TokenService = tokenService;
            _Client = new HttpClient();

            _Client.DefaultRequestHeaders.Accept.Clear();
            _Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _Client.DefaultRequestHeaders.AcceptCharset.Clear();
            _Client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("UTF-8"));
        }

        public async Task AddUserToGroup(string hub, string group, string userId)
        {
            if (string.IsNullOrEmpty(hub)) { throw new ArgumentNullException("Hub must have a value provided"); }
            if (string.IsNullOrEmpty(group)) { throw new ArgumentNullException("Group must have a value provided"); }
            if (string.IsNullOrEmpty(userId)) { throw new ArgumentNullException("UserId must have a value provided"); }

            await SendRequest(_EndPointService.RemoveUserFromGroup(hub, group, userId), hub, HttpMethod.Put);
        }

        public SignalrConnectionInfo GetClientConnection(string hub, string userId = null)
        {
            var connectionInfo = new SignalrConnectionInfo
            {
                AccessToken = _TokenService.GenerateClientAccessToken(hub, userId),
                Url = $"{_Endpoint}/client/?hub={hub}"
            };
            return connectionInfo;
        }

        public async Task RemoveUserFromGroup(string hub, string group, string userId)
        {
            if (string.IsNullOrEmpty(hub)) { throw new ArgumentNullException("Hub must have a value provided"); }
            if (string.IsNullOrEmpty(group)) { throw new ArgumentNullException("Group must have a value provided"); }
            if (string.IsNullOrEmpty(userId)) { throw new ArgumentNullException("UserId must have a value provided"); }

            await SendRequest(_EndPointService.RemoveUserFromGroup(hub, group, userId), hub, HttpMethod.Delete);
        }

        public async Task SendToAll(string hub, string targetMethod, object[] data)
        {
            if (string.IsNullOrEmpty(hub)) { throw new ArgumentNullException("Hub must have a value provided"); }
            if (string.IsNullOrEmpty(targetMethod)) { throw new ArgumentNullException("TargetMethod must have a value provided"); }

            await SendRequest(_EndPointService.SendToAll(hub), hub, HttpMethod.Post, targetMethod, data);
        }

        public async Task SendToGroup(string hub, string group, string targetMethod, object[] data)
        {
            if (string.IsNullOrEmpty(hub)) { throw new ArgumentNullException("Hub must have a value provided"); }
            if (string.IsNullOrEmpty(group)) { throw new ArgumentNullException("Group must have a value provided"); }
            if (string.IsNullOrEmpty(targetMethod)) { throw new ArgumentNullException("TargetMethod must have a value provided"); }

            await SendRequest(_EndPointService.SendToGroup(hub, group), hub, HttpMethod.Post, targetMethod, data);
        }

        public async Task SendToUser(string hub, string userId, string targetMethod, object[] data)
        {
            if (string.IsNullOrEmpty(hub)) { throw new ArgumentNullException("Hub must have a value provided"); }
            if (string.IsNullOrEmpty(userId)) { throw new ArgumentNullException("UserId must have a value provided"); }
            if (string.IsNullOrEmpty(targetMethod)) { throw new ArgumentNullException("TargetMethod must have a value provided"); }

            await SendRequest(_EndPointService.SendToUser(hub, userId), hub, HttpMethod.Post, targetMethod, data);
        }

        private async Task SendRequest(Uri uri, string hub, HttpMethod httpMethod, string targetMethod = null, object[] data = null)
        {
            StringContent content = null;
            if (data != null && data.Length != 0)
            {
                var contentString = JsonConvert.SerializeObject(new SignalrMessage
                {
                    Target = targetMethod,
                    Arguments = data
                });
                content = new StringContent(contentString, Encoding.UTF8, "application/json");
            }

            var request = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = uri,
                Content = content
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _TokenService.GenerateServerAccessToken(uri.AbsoluteUri));

            var response = await _Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Message was not successful for {uri}  {targetMethod}");
            }
        }
    }

    public class SignalrMessage
    {
        //Method Name that will invoke on the client(s) this is sent to.
        public string Target;
        public object[] Arguments;
    }

    public class SignalrConnectionInfo
    {
        public string Url { get; set; }
        public string AccessToken { get; set; }
    }
}
