using System.Xml.Serialization;
using System.Xml;

namespace FluentHttpClient.SoapMessaging;

public interface ISoapBody { }

[XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class SoapEnvelopeResponse
{
    [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public SoapBodyResponse Body { get; set; }
}

public class SoapBodyResponse
{
    [XmlAnyElement]
    public XmlElement[] Any { get; set; }
}
