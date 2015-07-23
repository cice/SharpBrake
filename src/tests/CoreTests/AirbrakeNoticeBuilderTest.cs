using System;
using System.Linq.Expressions;
using NUnit.Framework;
using SharpBrakeCore;
using SharpBrakeCore.Serialization;

namespace CoreTests
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

        private class AirbrakeNoticeBuilder : SharpBrakeCore.AirbrakeNoticeBuilder
        {
            public AirbrakeNoticeBuilder(IAirbrakeConfiguration configuration) : base(configuration)
            {
            }

            public AirbrakeError MakeError(Exception exception)
            {
                return Error(new ExceptionInformation(exception), new EnvironmentInformation(Configuration));
            }
        }

        [Test]
        public void Building_error_from_dotNET_exception()
        {
            var exception = Util.SimulateException();

            var error = _builder.MakeError(exception);
            Assert.That(error.Backtrace, Has.Length.GreaterThan(0));

            var trace = error.Backtrace[0];
            Assert.That(trace.Method, Is.EqualTo("SimulateException"));
            Assert.That(trace.LineNumber, Is.GreaterThan(0));
        }

        [Test]
        public void Notice_contains_ServerEnvironment_and_Notifier()
        {
            var notice = _builder.Notice(Util.SimulateException());
            Assert.That(notice.ServerEnvironment, Is.Not.Null);
            Assert.That(notice.ServerEnvironment.ProjectRoot, Is.Not.Null);
            Assert.That(notice.ServerEnvironment.EnvironmentName, Is.Not.Null);
            Assert.That(notice.Notifier, Is.Not.Null);
            Assert.That(notice.ApiKey, Is.Not.Empty);
            Assert.That(notice.Version, Is.Not.Null);
        }

        [Test]
        public void Notifier_initialized_correctly()
        {
            var notice = _builder.Notice(Util.SimulateException());
            var notifier = notice.Notifier;
            Assert.That(notifier.Name, Is.EqualTo("SharpBrakeCore"));
            Assert.That(notifier.Url, Is.EqualTo("https://github.com/kayoom/SharpBrake"));
            Assert.That(notifier.Version, Is.EqualTo("0.1.0.0"));
        }

        [Test]
        public void Server_environment_read_from_Airbrake_config()
        {
            var notice = _builder.Notice(Util.SimulateException());
            var environment = notice.ServerEnvironment;
            Assert.That(environment.EnvironmentName, Is.EqualTo(_config.EnvironmentName));
        }

        [Test]
        public void StackTrace_contains_lambda_expression()
        {
            Exception exception = null;

            try
            {
                Expression<Func<int>> inner = () => ((string) null).Length;

                inner.Compile()();
            }
            catch (Exception testException)
            {
                exception = testException;
            }

            var error = _builder.MakeError(exception);

            Assert.That(error, Is.Not.Null);
        }
    }
}