namespace SharpBrakeCore
{
    public class EnvironmentInformation : IEnvironmentInformation
    {
        private readonly IAirbrakeConfiguration _configuration;

        public EnvironmentInformation(IAirbrakeConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Uri
        {
            get { return "http://example.com/"; }
        }
    }
}