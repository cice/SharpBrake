using System.Configuration;
using System.Linq;
using System.Web;
using SharpBrakeCore;

namespace SharpBrakeWeb
{
    /// <summary>
    /// Configuration class for Airbrake.
    /// </summary>
    public class AirbrakeConfiguration : SharpBrakeCore.AirbrakeConfiguration
    {
        private static IAirbrakeConfiguration _default;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeConfiguration"/> class.
        /// </summary>
        private AirbrakeConfiguration()
        {
            ApiKey = ConfigurationManager.AppSettings["Airbrake.ApiKey"];
            EnvironmentName = ConfigurationManager.AppSettings["Airbrake.Environment"];
            var serverUri = ConfigurationManager.AppSettings["Airbrake.ServerUri"];

            if (serverUri != null)
                ServerUri = serverUri;

            var projectRoot = HttpRuntime.AppDomainAppVirtualPath;

            if (projectRoot != null)
                ProjectRoot = projectRoot;

            var values = ConfigurationManager.AppSettings.GetValues("Airbrake.AppVersion");

            if (values != null)
                AppVersion = values.FirstOrDefault();
        }

        public static IAirbrakeConfiguration Default
        {
            get { return _default ?? (_default = new AirbrakeConfiguration()); }
        }
    }
}