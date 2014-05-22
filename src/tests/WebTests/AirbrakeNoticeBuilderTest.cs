using System;
using System.IO;
using CoreTests;
using NUnit.Framework;
using SharpBrakeCore;
using SharpBrakeCore.Serialization;
using Subtext.TestLibrary;

namespace WebTests
{
    [TestFixture]
    public class AirbrakeNoticeBuilderTest
    {
        [SetUp]
        public void SetUp()
        {
            _config = new AirbrakeConfiguration
            {
                ApiKey = "123456",
                EnvironmentName = "test"
            };
            _builder = new AirbrakeNoticeBuilder(_config);
        }

        private AirbrakeConfiguration _config;
        private AirbrakeNoticeBuilder _builder;

        private class AirbrakeNoticeBuilder : SharpBrakeWeb.AirbrakeNoticeBuilder
        {
            public AirbrakeNoticeBuilder(IAirbrakeConfiguration configuration)
                : base(configuration)
            {
            }

            public AirbrakeError MakeError(Exception exception)
            {
                return Error(new ExceptionInformation(exception), new EnvironmentInformation(Configuration));
            }

            public AirbrakeNotifier MakeNotifier()
            {
                return Notifier;
            }
        }

        private static Exception SimulateException()
        {
            Exception exception;

            try
            {
                throw new InvalidOperationException("test error");
            }
            catch (Exception testException)
            {
                exception = testException;
            }
            return exception;
        }

        [Test]
        public void Notice_contains_Request()
        {
            AirbrakeNotice notice = null;
            const string url = "http://example.com/?Query.Key1=Query.Value1&Query.Key2=Query.Value2";
            const string referer = "http://github.com/";
            var physicalApplicationPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar;
            var httpSimulator = new HttpSimulator("/", physicalApplicationPath)
                .SetFormVariable("Form.Key1", "Form.Value1")
                .SetFormVariable("Form.Key2", "Form.Value2")
                .SetHeader("Header.Key1", "Header.Value1")
                .SetHeader("Header.Key2", "Header.Value2")
                .SetReferer(new Uri(referer))
                .SimulateRequest(new Uri(url));

            using (httpSimulator)
            {
                try
                {
                    Util.Throw(new Exception("Halp!"));
                }
                catch (Exception exception)
                {
                    notice = _builder.Notice(exception);
                }
            }

            Console.WriteLine(AbstractCleanXmlSerializer.ToXml(notice));

            Assert.That(notice, Is.Not.Null);
            Assert.That(notice.Error, Is.Not.Null);

            // We have defined a NET35 constant in the Visual Studio 2008 project so the below code isn't executed,
            // since it requires HttpSimulator which in turn requires .NET 4.0, which in turn requires Visual Studio 2010.
            Assert.That(notice.Request, Is.Not.Null);
            Assert.That(notice.Request.Url, Is.EqualTo(url));
            Assert.That(notice.Request.Component, Is.EqualTo(typeof (Util).FullName));
            Assert.That(notice.Request.Action, Is.EqualTo("Throw"));

            Assert.That(notice.Request.CgiData,
                Contains.Item(new AirbrakeVar("Content-Type", "application/x-www-form-urlencoded")));
            Assert.That(notice.Request.CgiData,
                Contains.Item(new AirbrakeVar("Header.Key1", "Header.Value1")));
            Assert.That(notice.Request.CgiData,
                Contains.Item(new AirbrakeVar("Header.Key2", "Header.Value2")));
            Assert.That(notice.Request.CgiData, Contains.Item(new AirbrakeVar("Referer", referer)));

            Assert.That(notice.Request.Params,
                Contains.Item(new AirbrakeVar("APPL_PHYSICAL_PATH", physicalApplicationPath)));
            Assert.That(notice.Request.Params,
                Contains.Item(new AirbrakeVar("QUERY_STRING", "Query.Key1=Query.Value1&Query.Key2=Query.Value2")));
            Assert.That(notice.Request.Params, Contains.Item(new AirbrakeVar("Form.Key1", "Form.Value1")));
            Assert.That(notice.Request.Params, Contains.Item(new AirbrakeVar("Form.Key2", "Form.Value2")));
            Assert.That(notice.Request.Params, Contains.Item(new AirbrakeVar("Query.Key1", "Query.Value1")));
            Assert.That(notice.Request.Params, Contains.Item(new AirbrakeVar("Query.Key2", "Query.Value2")));
        }

        [Test]
        public void Notifier_initialized_correctly()
        {
            var notifier = _builder.MakeNotifier();
            Assert.That(notifier.Name, Is.EqualTo("SharpBrakeWeb"));
            Assert.That(notifier.Url, Is.EqualTo("https://github.com/kayoom/SharpBrake"));
            Assert.That(notifier.Version, Is.EqualTo("2.2.1.0"));
        }
    }
}