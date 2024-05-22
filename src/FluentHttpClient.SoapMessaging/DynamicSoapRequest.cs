using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace FluentHttpClient.SoapMessaging;

public class DynamicSoapRequest : IXmlSerializable
{
    private Dictionary<string, object> _parameters = [];

    public DynamicSoapRequest()
    {
            
    }

    public DynamicSoapRequest(string customNamespace)
    {
        Namespace = customNamespace;
    }

    public string Namespace { get; set; }

    public void AddParameter(string name, object value)
    {
        _parameters[name] = value;
    }

    public System.Xml.Schema.XmlSchema GetSchema()
    {
        return null!;
    }

    public void ReadXml(XmlReader reader)
    {
        reader.ReadStartElement();
        while (reader.NodeType != XmlNodeType.EndElement)
        {
            string name = reader.Name;
            string value = reader.ReadElementContentAsString();
            _parameters[name] = value;
        }
        reader.ReadEndElement();
    }

    public void WriteXml(XmlWriter writer)
    {
        foreach (var param in _parameters)
        {
            //writer.WriteStartElement(param.Key, Namespace);
            writer.WriteStartElement(param.Key);
            writer.WriteValue(param.Value);
            writer.WriteEndElement();
        }
    }
}