using System;
using System.IO;
using System.Net;
using System.Text;
using Common.Logging;
using SharpBrakeCore.Serialization;

namespace SharpBrakeCore
{
    /// <summary>
    /// The client responsible for communicating exceptions to the Airbrake service.
    /// </summary>
    public class AirbrakeClient
    {
        private readonly IAirbrakeNoticeBuilder _builder;
        private readonly IAirbrakeConfiguration _configuration;
        private readonly ILog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeClient"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="builder">Optional: A different builder.</param>
        public AirbrakeClient(IAirbrakeConfiguration configuration, IAirbrakeNoticeBuilder builder = null)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            if (builder == null)
                builder = new AirbrakeNoticeBuilder(configuration);

            _configuration = configuration;
            _builder = builder;
            _log = LogManager.GetLogger(GetType());
        }


        /// <summary>
        /// Occurs when the request ends.
        /// </summary>
        public event RequestEndEventHandler RequestEnd;


        /// <summary>
        /// Sends the specified exception to Airbrake.
        /// </summary>
        /// <param name="exception">The e.</param>
        public void Send(Exception exception)
        {
            var notice = _builder.Notice(exception);

            //TODO: set up request, session and server headers
            // Why would that be necessary, it's set in Send(AirbrakeNotice), isn't it? - @asbjornu

            // Send the notice
            Send(notice);
        }


        /// <summary>
        /// Sends the specified notice to Airbrake.
        /// </summary>
        /// <param name="notice">The notice.</param>
        public void Send(AirbrakeNotice notice)
        {
            _log.Debug(f => f("{0}.Send({1})", GetType(), notice));

            try
            {
                // Create the web request
                var request = WebRequest.Create(_configuration.ServerUri) as HttpWebRequest;

                if (request == null)
                {
                    _log.Fatal(f => f("Couldn't create a request to '{0}'.", _configuration.ServerUri));
                    return;
                }

                // Set the basic headers
                request.ContentType = "text/xml";
                request.Accept = "text/xml";
                request.KeepAlive = false;

                // It is important to set the method late... .NET quirk, it will interfere with headers set after
                request.Method = "POST";

                // Go populate the body
                SetRequestBody(request, notice);

                // Begin the request, yay async
                request.BeginGetResponse(RequestCallback, request);
            }
            catch (Exception exception)
            {
                _log.Fatal("An error occurred while trying to send to Airbrake.", exception);
            }
        }


        private void OnRequestEnd(WebRequest request, WebResponse response)
        {
            if (response == null)
            {
                _log.Fatal(f => f("No response received!"));
                return;
            }

            string responseBody;

            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null)
                    return;

                using (var sr = new StreamReader(responseStream))
                {
                    responseBody = sr.ReadToEnd();
                    _log.Debug(f => f("Received from Airbrake.\n{0}", responseBody));
                }
            }

            if (RequestEnd == null)
                return;

            var e = new RequestEndEventArgs(request, response, responseBody);
            RequestEnd(this, e);
        }


        private void RequestCallback(IAsyncResult result)
        {
            _log.Debug(f => f("{0}.RequestCallback({1})", GetType(), result));

            // Get it back
            var request = result.AsyncState as HttpWebRequest;

            if (request == null)
            {
                _log.Fatal(
                    f => f(
                        "{0}.AsyncState was null or not of type {1}.",
                        typeof (IAsyncResult),
                        typeof (HttpWebRequest)));
                return;
            }

            WebResponse response;

            // We want to swallow any error responses
            try
            {
                response = request.EndGetResponse(result);
            }
            catch (WebException exception)
            {
                // Since an exception was already thrown, allowing another one to bubble up is pointless
                _log.Fatal("An error occurred while retrieving the web response", exception);
                response = exception.Response;
            }

            OnRequestEnd(request, response);
        }


        private void SetRequestBody(WebRequest request, AirbrakeNotice notice)
        {
            var serializer = new CleanXmlSerializer<AirbrakeNotice>();
            var xml = serializer.ToXml(notice);

            _log.Debug(f => f("Sending the following to '{0}':\n{1}", request.RequestUri, xml));

            var payload = Encoding.UTF8.GetBytes(xml);
            request.ContentLength = payload.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(payload, 0, payload.Length);
            }
        }
    }
}