using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Common.Logging;
using SharpBrakeCore.Serialization;

namespace SharpBrakeCore
{
    /// <summary>
    /// Responsible for building the notice that is sent to Airbrake.
    /// </summary>
    public class AirbrakeNoticeBuilder : IAirbrakeNoticeBuilder
    {
        protected readonly IAirbrakeConfiguration Configuration;
        protected readonly ILog Log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeNoticeBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public AirbrakeNoticeBuilder(IAirbrakeConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            Configuration = configuration;
            Log = LogManager.GetLogger(GetType());
            ServerEnvironment = new AirbrakeServerEnvironment(Configuration);
            Notifier = new AirbrakeNotifier
            {
                Name = "SharpBrakeCore",
                Url = "https://github.com/kayoom/SharpBrake",
                Version = typeof (AirbrakeNotice).Assembly.GetName().Version.ToString()
            };
        }

        /// <summary>
        /// Gets the notifier.
        /// </summary>
        protected AirbrakeNotifier Notifier { get; set; }

        /// <summary>
        /// Gets the server environment.
        /// </summary>
        protected AirbrakeServerEnvironment ServerEnvironment { get; set; }


        /// <summary>
        /// Creates a <see cref="AirbrakeError"/> from the the specified exception.
        /// </summary>
        /// <param name="exInfo"></param>
        /// <param name="envInfo"></param>
        /// <returns>
        /// A <see cref="AirbrakeError"/>, created from the the specified exception.
        /// </returns>
        protected AirbrakeError Error(IExceptionInformation exInfo, IEnvironmentInformation envInfo)
        {
            var error = Activator.CreateInstance<AirbrakeError>();

            error.CatchingMethod = exInfo.CatchingMethod;
            error.Class = exInfo.ExceptionClass.FullName;
            error.Message = exInfo.Message;
            error.Backtrace = exInfo.TraceLines.ToArray();

            return error;
        }


        /// <summary>
        /// Creates a <see cref="AirbrakeNotice"/> from the the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// A <see cref="AirbrakeNotice"/>, created from the the specified exception.
        /// </returns>
        public AirbrakeNotice Notice(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");
            Log.Info(f => f("{0}.Notice({1})", GetType(), exception.GetType()), exception);

            var exInfo = new ExceptionInformation(exception);
            var envInfo = new EnvironmentInformation(Configuration);

            return new AirbrakeNotice
            {
                Notifier = Notifier,
                ApiKey = Configuration.ApiKey,
                ServerEnvironment = ServerEnvironment,
                Error = Error(exInfo, envInfo),
                Request = Request(exInfo, envInfo)
            };
        }

        protected virtual AirbrakeRequest Request(IExceptionInformation exInfo, IEnvironmentInformation envInfo)
        {
            return new AirbrakeRequest(envInfo.Uri, exInfo.CatchingFile, exInfo.CatchingMethod.Name)
            {
                Url = Url(exInfo, envInfo),
                CgiData = CgiData(exInfo, envInfo).ToArray(),
                Params = Params(exInfo, envInfo).ToArray(),
                Session = Session(exInfo, envInfo).ToArray()
            };
        }

        protected virtual string Url(IExceptionInformation exInfo, IEnvironmentInformation envInfo)
        {
            return exInfo.CatchingFile;
        }

        protected virtual IEnumerable<AirbrakeVar> Session(IExceptionInformation exInfo, IEnvironmentInformation envInfo)
        {
            return Enumerable.Empty<AirbrakeVar>();
        }

        protected virtual IEnumerable<AirbrakeVar> Params(IExceptionInformation exInfo, IEnvironmentInformation envInfo)
        {
            return Enumerable.Empty<AirbrakeVar>();
        }

        protected virtual IEnumerable<AirbrakeVar> CgiData(IExceptionInformation exInfo, IEnvironmentInformation envInfo)
        {
            return Enumerable.Empty<AirbrakeVar>();
        }

        protected IEnumerable<AirbrakeVar> BuildVars(NameValueCollection nvCollection)
        {
            if ((nvCollection == null) || (nvCollection.Count == 0))
            {
                Log.Debug(f => f("No form data to build vars from."));
                return new AirbrakeVar[0];
            }

            return nvCollection.ToAirbrakeVars();
        }
    }
}