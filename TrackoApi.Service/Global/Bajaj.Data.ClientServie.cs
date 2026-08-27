using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using TrackoApi.Service.Bajaj.Data;
using TrackoApi.Service.Bajaj.Data.Response;

using TrackoApi.Data;

namespace TrackoApi.Service.Global
{
    public class BajajClientService
    {

        public BajajClientService()
        {
            
        }
        public async Task<TransportersResponseData> GetTransportersDataOutAsync()
        {
            try
            {
                var client = new RestClient("http://agni.bajajauto.co.in:7772/XISOAPAdapter/MessageServlet?senderParty=&senderService=BC_TransAPI&receiverParty=&receiverService=&interface=TransportersData_OUT&interfaceNamespace=http://bajajauto.co.in/TransAPI/TransportersData");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("SOAPAction", "\"http://sap.com/xi/WebService/soap1.1\"");
                request.AddHeader("Authorization", "Basic cGljb25uOmJhamFqQDEyMw==");
                request.AddHeader("Content-Type", "text/xml; charset=utf-8");
                request.AddHeader("Accept", "application/xml");
                var body = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tran=""http://bajajauto.co.in/TransAPI/TransportersData""><soapenv:Header/><soapenv:Body><tran:TransportersDataRequest><Header> <RequestTokenID>139936038</RequestTokenID> </Header></tran:TransportersDataRequest></soapenv:Body></soapenv:Envelope>";
                request.AddParameter("text/xml; charset=utf-8", body, ParameterType.RequestBody);
                request.XmlSerializer=new RestSharp.Serializers.DotNetXmlSerializer();
                var response =await client.ExecutePostTaskAsync(request);
                var content = new SoapEnvelopeSerializationProvider().ToSoapEnvelope(response.Content);
                return content.Body<TransportersResponseData>();
            }
            catch
            {
                throw;
            }
        }
        public async Task<TransportersDataResponse> GetTransportersDataResponseAsync()
        {
            try
            {
                var client = new RestClient("http://agni.bajajauto.co.in:7772/XISOAPAdapter/MessageServlet?senderParty=&senderService=BC_TransAPI&receiverParty=&receiverService=&interface=TransportersData_OUT&interfaceNamespace=http://bajajauto.co.in/TransAPI/TransportersData");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("SOAPAction", "\"http://sap.com/xi/WebService/soap1.1\"");
                request.AddHeader("Authorization", "Basic cGljb25uOmJhamFqQDEyMw==");
                request.AddHeader("Content-Type", "text/xml; charset=utf-8");
                request.AddHeader("Accept", "application/xml");
                var body = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tran=""http://bajajauto.co.in/TransAPI/TransportersData""><soapenv:Header/><soapenv:Body><tran:TransportersDataRequest><Header> <RequestTokenID>139936038</RequestTokenID> </Header></tran:TransportersDataRequest></soapenv:Body></soapenv:Envelope>";
                request.AddParameter("text/xml; charset=utf-8", body, ParameterType.RequestBody);
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                var response = await client.ExecutePostTaskAsync(request);
                var content = new SoapEnvelopeSerializationProvider().ToSoapEnvelope(response.Content);
                return content.Body<TransportersDataResponse>();
            }
            catch
            {
                throw;
            }
        }
        public async Task<TransportersData_OUTResponse> Test()
        {
            try
            {
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

                EndpointAddress endpoint = new EndpointAddress("http://agni.bajajauto.co.in:7772/XISOAPAdapter/MessageServlet?senderParty=&amp;senderService=BC_TransAPI&amp;receiverParty=&amp;receiverService=&amp;interface=TransportersData_OUT&amp;interfaceNamespace=http%3A%2F%2Fbajajauto.co.in%2FTransAPI%2FTransportersData");
                
                var client = new TransportersData_OUTClient(binding, endpoint);
                client.ClientCredentials.UserName.UserName = "piconn";
                client.ClientCredentials.UserName.Password = "bajaj@123";
                //var result = await client.TransportersData_OUTAsync(new TransportersDataRequest());
                var result = await client.TransportersData_OUTAsync(new TransportersDataRequest()
                {
                    Header = new[] { "<RequestTokenID>139936038</RequestTokenID>" }
                });
                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

    }
    /// <summary>
    /// Represents a SOAP Envelope
    /// </summary>
    [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class SoapEnvelope
    {
        /// <summary>
        /// The SOAP Envelope Header section
        /// </summary>
        [XmlElement("Header")]
        public SoapEnvelopeHeader Header { get; set; }

        /// <summary>
        /// The SOAP Envelope Body section
        /// </summary>
        [XmlElement("Body")]
        public SoapEnvelopeBody Body { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="SoapEnvelope"/>
        /// </summary>
        public SoapEnvelope()
        {
            Header = new SoapEnvelopeHeader();
            Body = new SoapEnvelopeBody();
        }

        /// <summary>
        /// Prepares a new SOAP Envelope to be manipulated
        /// </summary>
        /// <returns>The <see cref="SoapEnvelope"/> instance</returns>
        public static SoapEnvelope Prepare()
        {
            return new SoapEnvelope();
        }
    }
    /// <summary>
    /// Represents the SOAP Envelope Header section
    /// </summary>
    public class SoapEnvelopeHeader
    {
        /// <summary>
        /// The collection of headers
        /// </summary>
        [XmlAnyElement]
        public XElement[] Headers { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="SoapEnvelopeHeader"/>
        /// </summary>
        public SoapEnvelopeHeader()
        {
            Headers = new XElement[0];
        }
    }
    /// <summary>
    /// Represents the SOAP Envelope Body section
    /// </summary>
    public class SoapEnvelopeBody
    {
        /// <summary>
        /// The body content
        /// </summary>
        [XmlAnyElement]
        public XElement Value { get; set; }
    }
    /// <summary>
    /// Represents a SOAP Fault
    /// </summary>
    [XmlRoot("Fault", Namespace = "")]
    public class SoapFault
    {
        /// <summary>
        /// The fault code
        /// </summary>
        [XmlElement("faultcode", Namespace = "")]
        public string Code { get; set; }

        /// <summary>
        /// The fault string
        /// </summary>
        [XmlElement("faultstring", Namespace = "")]
        public string String { get; set; }

        /// <summary>
        /// The fault actor
        /// </summary>
        [XmlElement("faultactor", Namespace = "")]
        public string Actor { get; set; }

        /// <summary>
        /// The fault detail
        /// </summary>
        [XmlAnyElement("detail", Namespace = "")]
        public XElement Detail { get; set; }
    }
     /// <summary>
    /// Provider for serialization and deserialization of <see cref="SoapEnvelope"/> instances.
    /// </summary>
    public class SoapEnvelopeSerializationProvider
    {
        private XmlWriterSettings _xmlWriterSettings;
        private XmlSerializerNamespaces _xmlSerializerNamespaces;

        /// <summary>
        /// XML writer settings to be used when serializing <see cref="SoapEnvelope"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public XmlWriterSettings XmlWriterSettings
        {
            get { return _xmlWriterSettings; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _xmlWriterSettings = value;
            }
        }

        /// <summary>
        /// XML serializer namespaces to be used when serializing <see cref="SoapEnvelope"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public XmlSerializerNamespaces XmlSerializerNamespaces
        {
            get { return _xmlSerializerNamespaces; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _xmlSerializerNamespaces = value;
            }
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public SoapEnvelopeSerializationProvider()
        {
            _xmlWriterSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = false,
                NamespaceHandling = NamespaceHandling.OmitDuplicates
            };

            _xmlSerializerNamespaces = new XmlSerializerNamespaces();
            _xmlSerializerNamespaces.Add("", "");
        }

        #region Implementation of ISoapEnvelopeSerializationProvider

        /// <summary>
        /// Serializes a given <see cref="SoapEnvelope"/> instance into a XML string.
        /// </summary>
        /// <param name="envelope">The instance to serialize</param>
        /// <returns>The resulting XML string</returns>
        public string ToXmlString(SoapEnvelope envelope)
        {
            if (envelope == null) return null;

            try
            {
                using (var textWriter = new StringWriter())
                using (var xmlWriter = XmlWriter.Create(textWriter, XmlWriterSettings))
                {
                    new XmlSerializer(typeof(SoapEnvelope))
                        .Serialize(xmlWriter, envelope, XmlSerializerNamespaces);
                    return textWriter.ToString();
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// Deserializes a given XML string into a <see cref="SoapEnvelope"/>.
        /// </summary>
        /// <param name="xml">The XML string do deserialize</param>
        /// <returns>The resulting <see cref="SoapEnvelope"/></returns>
        public SoapEnvelope ToSoapEnvelope(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;

            try
            {
                using (var textWriter = new StringReader(xml))
                {
                    var result = (SoapEnvelope)new XmlSerializer(typeof(SoapEnvelope)).Deserialize(textWriter);

                    return result;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        #endregion
    }

     /// <summary>
     /// Helper methods for working with <see cref="SoapEnvelope"/> instances.
     /// </summary>
     public static class EnvelopeHelpers
     {
         private static readonly XmlSerializerNamespaces EmptyXmlSerializerNamespaces;

         static EnvelopeHelpers()
         {
             EmptyXmlSerializerNamespaces = new XmlSerializerNamespaces();
             EmptyXmlSerializerNamespaces.Add("", "");
         }
          #region Body

        /// <summary>
        /// Sets the given <see cref="XElement"/> as the envelope body.
        /// </summary>
        /// <param name="envelope">The <see cref="SoapEnvelope"/> to be used.</param>
        /// <param name="body">The <see cref="XElement"/> to set as the body.</param>
        /// <returns>The <see cref="SoapEnvelope"/> after changes.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static SoapEnvelope Body(this SoapEnvelope envelope, XElement body)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));

            if (envelope.Body == null)
                envelope.Body = new SoapEnvelopeBody();

            envelope.Body.Value = body;

            return envelope;
        }

        /// <summary>
        /// Sets the given entity as the envelope body.
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="envelope">The <see cref="SoapEnvelope"/> to be used.</param>
        /// <param name="body">The entity to set as the body.</param>
        /// <returns>The <see cref="SoapEnvelope"/> after changes.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static SoapEnvelope Body<T>(this SoapEnvelope envelope, T body)
        {
            return envelope.Body(body.ToXElement());
        }

        /// <summary>
        /// Extracts the <see cref="SoapEnvelope.Body"/> as an object of the given type.
        /// </summary>
        /// <typeparam name="T">The type do be deserialized.</typeparam>
        /// <param name="envelope">The <see cref="SoapEnvelope"/></param>
        /// <returns>The deserialized object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FaultException">Thrown if the body contains a fault</exception>
        public static T Body<T>(this SoapEnvelope envelope)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));


            var content= envelope?.Body?.Value?.ToString();
            if (string.IsNullOrEmpty(content))
            {
                return default;
            }

            if (content.TrimStart().StartsWith("<"))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);
                content = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.None, true);
                return JsonConvert.DeserializeObject<T>(content);
            }
            return default;
        }

        #endregion
        /// <summary>
        /// Serializes a given object to XML and returns the <see cref="XElement"/> representation.
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="item">The item to convert</param>
        /// <param name="removeXmlDeclaration">Remove the XML declaration</param>
        /// <returns>The object as a <see cref="XElement"/></returns>
        public static XElement ToXElement<T>(this T item, bool removeXmlDeclaration)
        {
            return item == null ? null : XElement.Parse(item.ToXmlString(removeXmlDeclaration));
        }
        /// <summary>
        /// Serializes a given object to XML and returns the <see cref="XElement"/> representation.
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="item">The item to convert</param>
        /// <returns>The object as a <see cref="XElement"/></returns>
        public static XElement ToXElement<T>(this T item)
        {
            return item.ToXElement(false);
        }/// <summary>
        /// Serializes the given object to a XML string
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="item">The item to serialize</param>
        /// <param name="removeXmlDeclaration">Remove the XML declaration</param>
        /// <returns>The XML string</returns>
        public static string ToXmlString<T>(this T item, bool removeXmlDeclaration)
        {
            if (item == null) return null;

            using (var textWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings
            {
                OmitXmlDeclaration = removeXmlDeclaration,
                Indent = false,
                NamespaceHandling = NamespaceHandling.OmitDuplicates
            }))
            {
#if NETSTANDARD2_0 || NET45
                if (Attribute.IsDefined(item.GetType(), typeof(System.Runtime.Serialization.DataContractAttribute)))
                {
                    var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
                    serializer.WriteObject(xmlWriter, item);
                    xmlWriter.Flush();
                    return textWriter.ToString();
                }
#endif
                new XmlSerializer(item.GetType())
                    .Serialize(xmlWriter, item, EmptyXmlSerializerNamespaces);
                return textWriter.ToString();
            }
        }
        /// <summary>
        /// Deserializes a given XML string to a new object of the expected type.
        /// If null or white spaces the default(T) will be returned;
        /// </summary>
        /// <typeparam name="T">The type to be deserializable</typeparam>
        /// <param name="xml">The XML string to deserialize</param>
        /// <returns>The deserialized object</returns>
        public static T ToObject<T>(this string xml)
        {
            


                if (string.IsNullOrWhiteSpace(xml)) return default(T);

                using (var stringReader = new StringReader(xml))
                using (var xmlReader = XmlReader.Create(stringReader))
                {
#if NETSTANDARD2_0 || NET45
                if (Attribute.IsDefined(typeof(T), typeof(System.Runtime.Serialization.DataContractAttribute)))
                {
                    var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
                    return (T)serializer.ReadObject(xmlReader);
                }
#endif
                    try
                    {
                        var result = (T) new XmlSerializer(typeof(T)).Deserialize(xmlReader);
                        return result;
                    }
                    catch
                    {
                        if (xml.TrimStart().StartsWith("<"))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(xml);
                            xml = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.None, true);
                        }

                        // Now you can load the JSON into a JObject
                        var jsonObject = JObject.Parse(xml);
                        return JsonConvert.DeserializeObject<T>(xml);
                        // var jsonPropertyNames = jsonObject.Properties().Select(p => p.Name).ToList();
                        // foreach (string name in jsonPropertyNames)
                        // {
                        //     Console.WriteLine(name);
                        // }
                    }
                }

                return default(T);
        }
        /// <summary>
        /// Deserializes a given <see cref="XElement"/> to a new object of the expected type.
        /// If null the default(T) will be returned.
        /// </summary>
        /// <typeparam name="T">The type to be deserializable</typeparam>
        /// <param name="xml">The <see cref="XElement"/> to deserialize</param>
        /// <returns>The deserialized object</returns>
        public static T ToObject<T>(this XElement xml)
        {
            return xml == null ? default(T) : xml.ToString().ToObject<T>();
        }
     }
}

