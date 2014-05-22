using SharpBrakeCore;

namespace SharpBrakeWeb
{
    public class AirbrakeClient : SharpBrakeCore.AirbrakeClient
    {
        public AirbrakeClient() : base(AirbrakeConfiguration.Default, new AirbrakeNoticeBuilder(AirbrakeConfiguration.Default))
        {
        }
    }
}
