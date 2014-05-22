using System;

namespace SharpBrakeCore
{
    public class AirbrakeConfiguration : IAirbrakeConfiguration
    {
        public const string ServerUriDefault = "https://api.airbrake.io/notifier_api/v2/notices";

        public AirbrakeConfiguration()
        {
            ProjectRoot = Environment.CurrentDirectory;
            ServerUri = ServerUriDefault;
        }

        public string AppVersion { get; set; }
        public string ApiKey { get; set; }
        public string ServerUri { get; set; }
        public string EnvironmentName { get; set; }
        public string ProjectRoot { get; set; }
    }
}