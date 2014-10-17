using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SharpBrakeCore.Serialization;

namespace SharpBrakeCore
{
    /// <summary>
    /// Contains the extension method to send exceptions to Airbrake.
    /// </summary>
    public static class Extensions
    {
        internal static IEnumerable<AirbrakeVar> ToAirbrakeVars(this NameValueCollection nvCollection)
        {
            return from key in nvCollection.AllKeys
                where !String.IsNullOrEmpty(key)
                let value = nvCollection[key]
                where !String.IsNullOrEmpty(value)
                select new AirbrakeVar(key, value);
        }

        /// <summary>
        /// Tries to invoke the <paramref name="getter"/>. Returns <c>default(TResult)</c>
        /// if the invocation fails.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="getter">The getter.</param>
        /// <returns>
        /// The value returned from <paramref name="getter"/> or <c>default(TResult)</c>
        /// if the invocation fails.
        /// </returns>
        internal static TResult TryGet<TObject, TResult>(this TObject instance, Func<TObject, TResult> getter)
        {
            try
            {
                return getter.Invoke(instance);
            }
            catch (Exception)
            {
                return default(TResult);
            }
        }
    }
}