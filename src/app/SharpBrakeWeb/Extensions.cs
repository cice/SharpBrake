using System;
using SharpBrakeCore;

namespace SharpBrakeWeb
{
    public static class Extensions
    {
        /// <summary>
        /// Sends the <paramref name="exception"/> to Airbrake.
        /// </summary>
        /// <param name="exception">The exception to send to Airbrake.</param>
        public static void SendToAirbrake(this Exception exception)
        {
            new AirbrakeClient().Send(exception);
        }
    }
}