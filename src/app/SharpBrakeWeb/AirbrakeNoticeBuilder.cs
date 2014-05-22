using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using SharpBrakeCore;
using SharpBrakeCore.Serialization;

namespace SharpBrakeWeb
{
    public class AirbrakeNoticeBuilder : SharpBrakeCore.AirbrakeNoticeBuilder
    {
        public AirbrakeNoticeBuilder(IAirbrakeConfiguration configuration) : base(configuration)
        {
            Notifier = new AirbrakeNotifier
            {
                Name = "SharpBrakeWeb",
                Url = "https://github.com/kayoom/SharpBrake",
                Version = typeof (AirbrakeNotice).Assembly.GetName().Version.ToString()
            };
        }
        
        protected virtual HttpContext HttpContext
        {
            get { return HttpContext.Current; }
        }

        protected virtual HttpRequest HttpRequest
        {
            get { return HttpContext.Request; }
        }

        protected override string Url(IExceptionInformation exInfo, IEnvironmentInformation envInfo)
        {
            return HttpRequest.Url.ToString();
        }

        protected override IEnumerable<AirbrakeVar> Params(IExceptionInformation exInfo, IEnvironmentInformation envInfo)
        {
            return BuildVars(HttpRequest.Params);
        }

        protected override IEnumerable<AirbrakeVar> CgiData(IExceptionInformation exInfo,
            IEnvironmentInformation envInfo)
        {
            return EnvironmentData(exInfo, envInfo).
                Concat(HeadersData(exInfo, envInfo)).
                Concat(CookiesData(exInfo, envInfo)).
                Concat(BrowserData(exInfo, envInfo));
        }

        protected virtual IEnumerable<AirbrakeVar> BrowserData(IExceptionInformation exInfo,
            IEnvironmentInformation envInfo)
        {
            if (HttpRequest.Browser == null)
                return Enumerable.Empty<AirbrakeVar>();

            return new[]
            {
                new AirbrakeVar("Browser.Browser", HttpRequest.Browser.Browser),
                new AirbrakeVar("Browser.ClrVersion", HttpRequest.Browser.ClrVersion),
                new AirbrakeVar("Browser.Cookies", HttpRequest.Browser.Cookies),
                new AirbrakeVar("Browser.Crawler", HttpRequest.Browser.Crawler),
                new AirbrakeVar("Browser.EcmaScriptVersion", HttpRequest.Browser.EcmaScriptVersion),
                new AirbrakeVar("Browser.JavaApplets", HttpRequest.Browser.JavaApplets),
                new AirbrakeVar("Browser.MajorVersion", HttpRequest.Browser.MajorVersion),
                new AirbrakeVar("Browser.MinorVersion", HttpRequest.Browser.MinorVersion),
                new AirbrakeVar("Browser.Platform", HttpRequest.Browser.Platform),
                new AirbrakeVar("Browser.W3CDomVersion", HttpRequest.Browser.W3CDomVersion)
            };
        }

        protected virtual IEnumerable<AirbrakeVar> CookiesData(IExceptionInformation exInfo,
            IEnvironmentInformation envInfo)
        {
            return BuildVars(HttpRequest.Cookies);
        }

        protected virtual IEnumerable<AirbrakeVar> HeadersData(IExceptionInformation exInfo,
            IEnvironmentInformation envInfo)
        {
            return BuildVars(HttpRequest.Headers);
        }

        protected override IEnumerable<AirbrakeVar> Session(IExceptionInformation exInfo,
            IEnvironmentInformation envInfo)
        {
            return BuildVars(HttpContext.Session);
        }

        protected virtual IEnumerable<AirbrakeVar> EnvironmentData(IExceptionInformation exInfo,
            IEnvironmentInformation envInfo)
        {
            return new[]
            {
                new AirbrakeVar("Environment.MachineName", Environment.MachineName),
                new AirbrakeVar("Environment.OSversion", Environment.OSVersion),
                new AirbrakeVar("Environment.Version", Environment.Version)
            };
        }

        protected IEnumerable<AirbrakeVar> BuildVars(HttpCookieCollection cookies)
        {
            if ((cookies == null) || (cookies.Count == 0))
            {
                Log.Debug(f => f("No cookies to build vars from."));
                return new AirbrakeVar[0];
            }

            return from key in cookies.Keys.Cast<string>()
                where !String.IsNullOrEmpty(key)
                let cookie = cookies[key]
                let value = cookie != null ? cookie.Value : null
                where !String.IsNullOrEmpty(value)
                select new AirbrakeVar(key, value);
        }

        protected IEnumerable<AirbrakeVar> BuildVars(HttpSessionState session)
        {
            if ((session == null) || (session.Count == 0))
            {
                Log.Debug(f => f("No session to build vars from."));
                return new AirbrakeVar[0];
            }

            return from key in session.Keys.Cast<string>()
                where !String.IsNullOrEmpty(key)
                let v = session[key]
                let value = v != null ? v.ToString() : null
                where !String.IsNullOrEmpty(value)
                select new AirbrakeVar(key, value);
        }
    }
}