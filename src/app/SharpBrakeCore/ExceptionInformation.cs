using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SharpBrakeCore.Serialization;

namespace SharpBrakeCore
{
    public class ExceptionInformation : IExceptionInformation
    {
        public ExceptionInformation(Exception exception)
        {
            Exception = exception;
            StackTrace = new StackTrace(exception);
            ExceptionClass = exception.GetType();
            Message = ExceptionClass.Name + ": " + exception.Message;

            ProcessStackTrace();
        }

        public string Message { get; protected set; }
        public StackTrace StackTrace { get; protected set; }
        public Exception Exception { get; protected set; }
        public Type ExceptionClass { get; protected set; }
        public IEnumerable<AirbrakeTraceLine> TraceLines { get; protected set; }
        public MethodBase CatchingMethod { get; protected set; }
        public String CatchingFile { get; protected set; }

        private void ProcessStackTrace()
        {
            var frames = StackTrace.GetFrames();
            CatchingFile = String.Empty;

            if (frames == null || frames.Length == 0)
            {
                CatchingMethod = GetEntryPoint();
                if (CatchingMethod.DeclaringType != null)
                    CatchingFile = CatchingMethod.DeclaringType.FullName;
                TraceLines = new[] {AirbrakeTraceLine.Empty};
            }
            else
            {
                var topFrame = frames.First();
                CatchingMethod = topFrame.GetMethod();
                CatchingFile = CatchingMethod.DeclaringType != null
                    ? CatchingMethod.DeclaringType.FullName
                    : topFrame.GetFileName();
                TraceLines = frames.Select(TraceLineForFrame);
            }
        }

        private static AirbrakeTraceLine TraceLineForFrame(StackFrame frame)
        {
            var method = frame.GetMethod();

            var lineNumber = frame.GetFileLineNumber();
            if (lineNumber == 0)
                lineNumber = frame.GetILOffset();

            var fileName = frame.GetFileName();
            if (string.IsNullOrEmpty(fileName))
                fileName = method.ReflectedType != null ? method.ReflectedType.FullName : "(unknown)";

            return new AirbrakeTraceLine(fileName, lineNumber, method.Name);
        }

        private static MethodBase GetEntryPoint()
        {
            var assembly = Assembly.GetExecutingAssembly();

            if (assembly.EntryPoint == null)
                assembly = Assembly.GetCallingAssembly();

            if (assembly.EntryPoint == null)
                assembly = Assembly.GetEntryAssembly();

            return assembly == null
                ? null
                : assembly.EntryPoint;
        }
    }
}