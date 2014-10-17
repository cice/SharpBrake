using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using Common.Logging;
using SharpBrakeCore.Serialization;

namespace SharpBrakeCore
{
    /// <summary>
    /// The response received from Airbrake.
    /// </summary>
    public class AirbrakeResponse
    {
        private readonly string _content;
        private readonly long _contentLength;
        private readonly string _contentType;
        private readonly WebHeaderCollection _headers;
        private readonly bool _isFromCache;
        private readonly bool _isMutuallyAuthenticated;
        private readonly Uri _responseUri;
        private AirbrakeResponseError[] _errors;


        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeResponse"/> class.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="content">The content.</param>
        public AirbrakeResponse(WebResponse response, string content)
        {
            var log = LogManager.GetLogger(GetType());
            _content = content;
            _errors = new AirbrakeResponseError[0];

            if (response != null)
            {
                // TryGet is needed because the default behavior of WebResponse is to throw NotImplementedException
                // when a method isn't overridden by a deriving class, instead of declaring the method as abstract.
                _contentLength = response.TryGet(x => x.ContentLength);
                _contentType = response.TryGet(x => x.ContentType);
                _headers = response.TryGet(x => x.Headers);
                _isFromCache = response.TryGet(x => x.IsFromCache);
                _isMutuallyAuthenticated = response.TryGet(x => x.IsMutuallyAuthenticated);
                _responseUri = response.TryGet(x => x.ResponseUri);
            }

            try
            {
                Deserialize(content);
            }
            catch (Exception exception)
            {
                log.Fatal(f => f(
                    "An error occurred while deserializing the following content:\n{0}", content),
                    exception);
            }
        }


        /// <summary>
        /// Gets the content.
        /// </summary>
        public string Content
        {
            get { return _content; }
        }

        /// <summary>
        /// Gets the length of the content.
        /// </summary>
        /// <value>
        /// The length of the content.
        /// </value>
        public long ContentLength
        {
            get { return _contentLength; }
        }

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <value>
        /// The type of the content.
        /// </value>
        public string ContentType
        {
            get { return _contentType; }
        }

        /// <summary>
        /// Gets the errors.
        /// </summary>
        public AirbrakeResponseError[] Errors
        {
            get { return _errors; }
        }

        /// <summary>
        /// Gets the headers.
        /// </summary>
        public WebHeaderCollection Headers
        {
            get { return _headers; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is from cache.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is from cache; otherwise, <c>false</c>.
        /// </value>
        public bool IsFromCache
        {
            get { return _isFromCache; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is mutually authenticated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is mutually authenticated; otherwise, <c>false</c>.
        /// </value>
        public bool IsMutuallyAuthenticated
        {
            get { return _isMutuallyAuthenticated; }
        }

        /// <summary>
        /// Gets the notice returned from Airbrake.
        /// </summary>
        public AirbrakeResponseNotice Notice { get; private set; }

        /// <summary>
        /// Gets the response URI.
        /// </summary>
        public Uri ResponseUri
        {
            get { return _responseUri; }
        }


        private void Deserialize(string xml)
        {
            using (var stringReader = new StringReader(xml))
            {
                using (var reader = XmlReader.Create(stringReader))
                {
                    reader.MoveToContent();

                    switch (reader.LocalName)
                    {
                        case "errors":
                            _errors = BuildErrors(reader).ToArray();
                            break;

                        case "notice":
                            Notice = BuildNotice(reader);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Builds the errors from the <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>
        /// An <see cref="IEnumerable{AirbrakeResponseError}"/>.
        /// </returns>
        private static IEnumerable<AirbrakeResponseError> BuildErrors(XmlReader reader)
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.LocalName == "error")
                            yield return new AirbrakeResponseError(reader.ReadElementContentAsString());
                        break;
                }
            }
        }

        /// <summary>
        /// Builds the notice from the <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>
        /// A new instance of <see cref="AirbrakeResponseNotice"/>.
        /// </returns>
        private static AirbrakeResponseNotice BuildNotice(XmlReader reader)
        {
            var id = 0;
            var errorId = 0;
            string url = null;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "id":
                                id = reader.ReadElementContentAsInt();
                                break;

                            case "error-id":
                                errorId = reader.ReadElementContentAsInt();
                                break;

                            case "url":
                                url = reader.ReadElementContentAsString();
                                break;
                        }
                        break;
                }
            }

            return new AirbrakeResponseNotice
            {
                Id = id,
                ErrorId = errorId,
                Url = url,
            };
        }
    }
}