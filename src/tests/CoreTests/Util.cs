using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace CoreTests
{
    public static class Util
    {
        public static Exception SimulateException()
        {
            Exception exception;

            try
            {
                throw new InvalidOperationException("test error");
            }
            catch (Exception testException)
            {
                exception = testException;
            }
            return exception;
        }

        public static void ValidateSchema(string xml)
        {
            var schema = GetXmlSchema();

            var settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

            settings.Schemas.Add(schema);

            using (var reader = new StringReader(xml))
            using (var xmlReader = new XmlTextReader(reader))
            using (var validator = XmlReader.Create(xmlReader, settings))
                while (validator.Read()) ;
        }

        private static XmlSchema GetXmlSchema()
        {
            var assembly = typeof (Util).Assembly;
            const string xsd = "airbrake_2_2.xsd";

            using (var schemaStream = assembly.GetManifestResourceStream("CoreTests." + xsd))
            {
                if (schemaStream == null)
                    Assert.Fail("{0}.{1} not found.", assembly.FullName, xsd);

                return XmlSchema.Read(schemaStream, null);
            }
        }

        public static void Throw(Exception exception)
        {
            throw exception;
        }
    }
}