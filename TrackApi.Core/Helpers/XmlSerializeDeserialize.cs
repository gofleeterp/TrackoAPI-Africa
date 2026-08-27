using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace TrackoApi.Core.Helpers
{
    
    public class XmlSerializeDeserialize<T>
    {
        private static readonly Regex DtCheck = new Regex(@"(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2})([\+|-]\d{2}:\d{2})");
        private readonly StringBuilder _sbData;
        //private StringWriter _swWriter;
        private XmlDocument _xDoc;
        private XmlNodeReader _xNodeReader;
        private XmlSerializer _xmlSerializer;
        public XmlSerializeDeserialize()
        {
            _sbData = new StringBuilder();
        }
        public string SerializeData(T data)
        {
            string xml;
            if (data == null) return null;
            XmlSerializer employeeSerializer = new XmlSerializer(typeof(T));
            using (var swWriter = new StringWriter(_sbData))
            {
                employeeSerializer.Serialize(swWriter, data);
                xml = _sbData.ToString();
            }
            if (DtCheck.IsMatch(xml))
                xml = DtCheck.Replace(xml, "$1");
            return xml;
        }

        public T DeserializeData(string dataXml)
        {
            if (string.IsNullOrWhiteSpace(dataXml)) return default(T);
            _xDoc = new XmlDocument();
            _xDoc.LoadXml(dataXml);
            _xNodeReader = new XmlNodeReader(_xDoc.DocumentElement ?? throw new InvalidOperationException());
            _xmlSerializer = new XmlSerializer(typeof(T));
            var employeeData = _xmlSerializer.Deserialize(_xNodeReader);
            T deserializedEmployee = (T)employeeData;
            return deserializedEmployee;
        }
    }
}
