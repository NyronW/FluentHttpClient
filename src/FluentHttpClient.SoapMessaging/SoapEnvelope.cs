using System.Xml.Serialization;
using System.Xml;

namespace FluentHttpClient.SoapMessaging;

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