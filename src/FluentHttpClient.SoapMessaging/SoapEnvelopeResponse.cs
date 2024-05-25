using System.Xml.Serialization;
using System.Xml;

namespace FluentHttpClient.SoapMessaging;

public interface ISoapResponseBody { }

[XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class SoapEnvelopeResponse
{
    [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public SoapResponseBody Body { get; set; }
}

public class SoapResponseBody
{
    [XmlAnyElement]
    public XmlElement[] Any { get; set; }
}
