using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Ftpb_ValidateForm
{
    public class ValidationResult
    {
        public ValidationResult()
        {
            Messages = new List<string>();
        }
        public List<string> Messages { get; set; }
        public void AddWarning(string message)
        {
            Messages.Add(message);
        }

        public void AddError(string message)
        {
            Messages.Add(message);
        }
    }
    public class XmlUtil
    {
        private const int _validationErrorCountLimit = 100;
        private readonly ValidationResult _validationErrorMessages = new ValidationResult();

        public ValidationResult Validate(string xmlString, string xmlSchemaString)
        {
            var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));
            var xmlSchemaStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlSchemaString));
            return Validate(xmlStream, xmlSchemaStream);
        }

        public ValidationResult Validate(Stream xmlStream, Stream xmlSchemaStream)
        {
            XmlSchema xmlSchema = XmlSchema.Read(xmlSchemaStream, ValidationCallBack);
            XmlReaderSettings xmlReaderSettings = SetupXmlValidation(new List<XmlSchema> { xmlSchema });
            Validate(xmlStream, xmlReaderSettings);

            return _validationErrorMessages;
        }

        public ValidationResult Validate(Stream xmlStream, Stream[] xmlSchemaStreams)
        {
            var xmlSchemas = new List<XmlSchema>();

            foreach (Stream xmlSchemaStream in xmlSchemaStreams)
                xmlSchemas.Add(XmlSchema.Read(xmlSchemaStream, ValidationCallBack));

            XmlReaderSettings xmlReaderSettings = SetupXmlValidation(xmlSchemas);
            Validate(xmlStream, xmlReaderSettings);

            return _validationErrorMessages;
        }

        private void Validate(Stream xmlStream, XmlReaderSettings xmlReaderSettings)
        {
            using (XmlReader validationReader = XmlReader.Create(xmlStream, xmlReaderSettings))
            {
                try
                {
                    while (validationReader.Read())
                        if (_validationErrorMessages.Messages.Count >= _validationErrorCountLimit)
                            break;
                }
                catch (XmlException ex)
                {
                    _validationErrorMessages.AddError(ex.Message);
                }
            }
        }

        private XmlReaderSettings SetupXmlValidation(IEnumerable<XmlSchema> xmlSchemas)
        {
            var settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += ValidationCallBack;

            foreach (XmlSchema xmlSchema in xmlSchemas)
            {
                settings.Schemas.Add(xmlSchema);
            }

            return settings;
        }

        private void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                _validationErrorMessages.AddWarning("linje " + args.Exception.LineNumber + ", posisjon " + args.Exception.LinePosition + " " + args.Message);
            }
            else if (args.Severity == XmlSeverityType.Error)
            {
                _validationErrorMessages.AddError("linje " + args.Exception.LineNumber + ", posisjon " + args.Exception.LinePosition + " " + args.Message);
            }
        }

        private static string RemoveAllNamespaces(string xmlDocument)
        {
            XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));

            return xmlDocumentWithoutNs.ToString();
        }

        //Core recursion function
        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }

        public static string GetElementValue(string xmlString, string elementName)
        {
            try
            {
                XDocument document = XDocument.Parse(RemoveAllNamespaces(xmlString));
                var node = document.Descendants().Where(e => e.Name.LocalName.ToLower().Equals(elementName.ToLower())).FirstOrDefault();

                return node?.Value?.Trim();

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
