using System.Xml.Serialization;
using System.Xml;

namespace FluentHttpClient.SoapMessaging;

//[XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
//public class SoapEnvelope<T> where T : IXmlSerializable
//{
//    private List<XmlQualifiedName> _xmlQualifiedNames = 
//        [
//                new XmlQualifiedName("soap", "http://schemas.xmlsoap.org/soap/envelope/"),
//                new XmlQualifiedName("xsi", "http://www.w3.org/2001/XMLSchema-instance"),
//                new XmlQualifiedName("xsd", "http://www.w3.org/2001/XMLSchema"),
//        ];

//    public SoapEnvelope()
//    {

//    }

//    [XmlNamespaceDeclarations]
//    public XmlSerializerNamespaces Xmlns { get; set; }

//    [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
//    public SoapBody<T> Body { get; set; }

//    public SoapEnvelope(string customNamespace)
//    {
//        Xmlns = new XmlSerializerNamespaces(
//            new[] {
//                new XmlQualifiedName("soap", "http://schemas.xmlsoap.org/soap/envelope/"),
//                new XmlQualifiedName("xsi", "http://www.w3.org/2001/XMLSchema-instance"),
//                new XmlQualifiedName("xsd", "http://www.w3.org/2001/XMLSchema"),
//                new XmlQualifiedName("", customNamespace) // Default namespace
//            });
//        Body = new SoapBody<T>(customNamespace);
//    }
//}

//public class SoapBody<T> where T : IXmlSerializable
//{
//    [XmlElement(ElementName = "dynamic", Namespace ="yafl.soap")]
//    public T Request { get; set; } = default!;

//    [XmlAttribute("xmlns")]
//    public string CustomNamespace { get; set; } = string.Empty;

//    public SoapBody() { }

//    public SoapBody(string customNamespace)
//    {
//        CustomNamespace = customNamespace;
//    }
//}

[XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class SoapEnvelope
{
    [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public SoapBody Body { get; set; }
}

public class SoapBody
{
    [XmlAnyElement]
    public XmlElement[] Any { get; set; }
}