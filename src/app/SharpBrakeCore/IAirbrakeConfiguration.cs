namespace SharpBrakeCore
{
    public interface IAirbrakeConfiguration
    {
        /// <summary>
        /// Gets or sets the app version.
        /// </summary>
        /// <value>
        /// The app version.
        /// </value>
        string AppVersion { get; }

        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        string ApiKey { get; }

        /// <summary>
        /// Gets or sets the AirBrake server location.
        /// </summary>
        string ServerUri { get; }

        /// <summary>
        /// Gets or sets the name of the environment.
        /// </summary>
        /// <value>
        /// The name of the environment.
        /// </value>
        string EnvironmentName { get; }


        /// <remarks>
        /// Only set this if you need to override the default project root.
        /// </remarks>
        /// <value>
        /// The project root.
        /// </value>
        string ProjectRoot { get; }
    }
}