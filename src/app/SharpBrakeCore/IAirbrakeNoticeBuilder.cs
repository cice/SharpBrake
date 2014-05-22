using System;
using SharpBrakeCore.Serialization;

namespace SharpBrakeCore
{
    public interface IAirbrakeNoticeBuilder
    {
        /// <summary>
        /// Creates a <see cref="AirbrakeNotice"/> from the the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// A <see cref="AirbrakeNotice"/>, created from the the specified exception.
        /// </returns>
        AirbrakeNotice Notice(Exception exception);
    }
}