using Functions.SignalR.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Functions.SignalR
{
    public static class Installer
    {
        public static IServiceCollection AddServices(this ServiceCollection serviceCollection, string connectionString, TimeSpan tokenDuration, bool requireIdentity = true)
        {
            //Parse connection string and validate it.
            (var EndPoint, var AccessKey, var Version) = ParseConnectionString(connectionString);

            serviceCollection.AddSingleton<ISignalrService>(
                new SignalrService(EndPoint, AccessKey, 
                new TokenService(EndPoint, AccessKey, tokenDuration, requireIdentity),
                new EndPointService(EndPoint)));

            return serviceCollection;
        }

        /// <summary>
        /// Parses the connection string (that the SignalRService provides) into a tuple.
        /// </summary>
        private static (string EndPoint, string AccessKey, string Version) ParseConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("SignalR Service connection string is empty.  Check your AppSettings/Config to verify it is assigned.");
            }

            var endpointMatch = Regex.Match(connectionString, @"endpoint=([^;]+)", RegexOptions.IgnoreCase);
            if (!endpointMatch.Success)
            {
                throw new ArgumentException("No endpoint present in SignalR Service connection string");
            }
            var accessKeyMatch = Regex.Match(connectionString, @"accesskey=([^;]+)", RegexOptions.IgnoreCase);
            if (!accessKeyMatch.Success)
            {
                throw new ArgumentException("No access key present in SignalR Service connection string");
            }
            var versionKeyMatch = Regex.Match(connectionString, @"Version=([^;]+)", RegexOptions.IgnoreCase);

            if (versionKeyMatch.Success && !System.Version.TryParse(versionKeyMatch.Groups[1].Value, out Version version))
            {
                throw new ArgumentException("Invalid version format in SignalR Service connection string");
            }

            return (endpointMatch.Groups[1].Value, accessKeyMatch.Groups[1].Value, versionKeyMatch.Groups[1].Value);
        }
    }
}
