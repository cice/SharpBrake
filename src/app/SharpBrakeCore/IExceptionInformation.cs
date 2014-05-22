using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using SharpBrakeCore.Serialization;

namespace SharpBrakeCore
{
    public interface IExceptionInformation
    {
        string Message { get; }
        StackTrace StackTrace { get; }
        Exception Exception { get; }
        Type ExceptionClass { get; }
        IEnumerable<AirbrakeTraceLine> TraceLines { get; }
        MethodBase CatchingMethod { get; }
        String CatchingFile { get; }
    }
}