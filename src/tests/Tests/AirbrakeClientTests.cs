using System;
using NUnit.Framework;
using SharpBrake.Serialization;

namespace SharpBrake.Tests
{
    [TestFixture]
    public class AirbrakeClientTests
    {
        [SetUp]
        public void SetUp()
        {
            client = new AirbrakeClient();
        }

        private AirbrakeClient client;


        [Test]
        [Ignore("This test needs to be rewritten for the 2.2 API")]
        public void Send_EndRequestEventIsInvoked_And_ResponseOnlyContainsApiError()
        {
            var requestEndInvoked = false;
            AirbrakeResponseError[] errors = null;

            client.RequestEnd += (sender, e) =>
            {
                requestEndInvoked = true;
                errors = e.Response.Errors;
            };

            var configuration = new AirbrakeConfiguration
            {
                ApiKey = Guid.NewGuid().ToString("N"),
                EnvironmentName = "test",
            };

            var builder = new AirbrakeNoticeBuilder(configuration);

            var notice = builder.Notice(new Exception("Test"));

            notice.Request = new AirbrakeRequest("http://example.com", "Test")
            {
                Params = new[]
                {
                    new AirbrakeVar("TestKey", "TestValue")
                }
            };

            client.Send(notice);

            Assert.That(requestEndInvoked, Is.True.After(5000));
            Assert.That(errors, Is.Not.Null);
            Assert.That(errors, Has.Length.EqualTo(1));
        }
    }
}