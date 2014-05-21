using System;
using System.Net;

namespace SharpBrake
{
    /// <summary>
    /// The event arguments passed to <see cref="RequestEndEventHandler"/>.
    /// </summary>
    [Serializable]
    public class RequestEndEventArgs : EventArgs
    {
        private readonly WebRequest _request;
        private readonly AirbrakeResponse _response;


        /// <summary>
        /// Initializes a new instance of the <see cref="RequestEndEventArgs"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="content">The body of the response.</param>
        public RequestEndEventArgs(WebRequest request, WebResponse response, string content)
        {
            _request = request;
            _response = new AirbrakeResponse(response, content);
        }


        /// <summary>
        /// Gets the request.
        /// </summary>
        public WebRequest Request
        {
            get { return _request; }
        }

        /// <summary>
        /// Gets the response.
        /// </summary>
        public AirbrakeResponse Response
        {
            get { return _response; }
        }
    }
}