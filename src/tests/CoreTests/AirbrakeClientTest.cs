using System;
using NUnit.Framework;
using SharpBrakeCore;
using SharpBrakeCore.Serialization;

namespace CoreTests
{
    [TestFixture]
    public class AirbrakeClientTest
    {
        [SetUp]
        public void SetUp()
        {
            _client = new AirbrakeClient(new AirbrakeConfiguration());
        }

        private AirbrakeClient _client;

        [Test]
        [Ignore("This test needs to be rewritten for the 2.2 API")]
        public void Send_EndRequestEventIsInvoked_And_ResponseOnlyContainsApiError()
        {
            var requestEndInvoked = false;
            AirbrakeResponseError[] errors = null;

            _client.RequestEnd += (sender, e) =>
            {
                requestEndInvoked = true;
                errors = e.Response.Errors;
            };

            var configuration = new AirbrakeConfiguration
            {
                ApiKey = Guid.NewGuid().ToString("N"),
                EnvironmentName = "test",
                ProjectRoot = Environment.CurrentDirectory
            };

            var builder = new AirbrakeNoticeBuilder(configuration);

            var notice = builder.Notice(Util.SimulateException());

            notice.Request = new AirbrakeRequest("http://example.com", "Test", "test")
            {
                Params = new[]
                {
                    new AirbrakeVar("TestKey", "TestValue")
                }
            };

            _client.Send(notice);

            Assert.That(requestEndInvoked, Is.True.After(5000));
            Assert.That(errors, Is.Not.Null);
            Assert.That(errors, Has.Length.EqualTo(1));
        }
    }
}