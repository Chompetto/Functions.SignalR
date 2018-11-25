using System;
using System.Collections.Generic;
using System.Text;

namespace Functions.SignalR.Services
{
    public interface IEndPointService
    {
        Uri SendToAll(string hub);
        Uri SendToGroup(string hub, string group);
        Uri SendToUser(string hub, string userId);
        Uri AddUserToGroup(string hub, string group, string userId);
        Uri RemoveUserFromGroup(string hub, string group, string userId);
    }
    public class EndPointService : IEndPointService
    {
        private readonly string _EndPoint;

        public EndPointService(string endPoint)
        {
            _EndPoint = endPoint;
        }

        public Uri AddUserToGroup(string hub, string group, string userId)
        {
            return new Uri($"{_EndPoint}/api/v1/hubs/{hub}/groups/{group}/users/{userId}");
        }

        public Uri RemoveUserFromGroup(string hub, string group, string userId)
        {
            return new Uri($"{_EndPoint}/api/v1/hubs/{hub}/groups/{group}/users/{userId}");
        }

        public Uri SendToAll(string hub)
        {
            return new Uri($"{_EndPoint}/api/v1/hubs/{hub}");
        }

        public Uri SendToGroup(string hub, string group)
        {
            return new Uri($"{_EndPoint}/api/v1/hubs/{hub}/groups/{group}");
        }

        public Uri SendToUser(string hub, string userId)
        {
            return new Uri($"{_EndPoint}/api/v1/hubs/{hub}/users/{userId}");
        }
    }
}
