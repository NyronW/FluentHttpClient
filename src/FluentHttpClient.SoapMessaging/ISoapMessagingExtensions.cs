using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Reflection;

namespace FluentHttpClient.SoapMessaging;

public static class ISoapMessagingExtensions
{
    public static async Task<HttpResponseMessage> SoapPostAsync<TRequest>(this ISendRequestWithBody client, TRequest request, string methodName, string customNamespace)
    {
        HttpContent content = BuildSoapContent(request, methodName, customNamespace);

        return await ((ISetContentType)client).UsingContentType("text/xml")
            .PostAsync(content);
    }

    public static async Task<TResponse> SoapPostAsync<TRequest, TResponse>(this ISendRequestWithBody client, TRequest request, string methodName,
        string customNamespace, string responseElementName = "") where TResponse : class, ISoapBody
    {
        var response = await client.SoapPostAsync(request, methodName, customNamespace);
        string result = await response.Content.ReadAsStringAsync();

        var responseObject = DeserializeSoapResponse<TResponse>(result, responseElementName, customNamespace);

        return responseObject;
    }


    private static StringContent BuildSoapContent<TRequest>(TRequest request, string methodName, string customNamespace)
    {
        var dynamicRequest = new DynamicSoapRequest(customNamespace);
        // Use reflection to add all public properties from the TRequest object to the dynamicRequest object
        foreach (var property in typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = property.GetValue(request);
            dynamicRequest.AddParameter(property.Name, value!);
        }

        var soapEnvelope = new SoapEnvelope<DynamicSoapRequest>(customNamespace)
        {
            Body = new SoapBody<DynamicSoapRequest>(customNamespace)
            {
                Request = dynamicRequest,
            }
        };

        var xmlSerializer = new XmlSerializer(typeof(SoapEnvelope<DynamicSoapRequest>));
        var settings = new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = true };

        string xmlRequest;
        using (var stream = new MemoryStream())
        using (var writer = XmlWriter.Create(stream, settings))
        {
            xmlSerializer.Serialize(writer, soapEnvelope);
            xmlRequest = Encoding.UTF8.GetString(stream.ToArray());
        }

        // Manually modify the serialized XML to insert the correct method name
        xmlRequest = xmlRequest.Replace($"<dynamic xmlns=\"yafl.soap\">", $"<{methodName} xmlns=\"{customNamespace}\">")
                               .Replace("</dynamic>", $"</{methodName}>");

        var content = new StringContent(xmlRequest, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", $"\"{customNamespace}{methodName}\"");
        return content;
    }

    //private static TResponse DeserializeSoapResponse<TResponse>(string soapResponse, string customNamespace, string methodName)
    //    where TResponse : ISoapBody
    //{
    //    using var reader = new StringReader(soapResponse);

    //    var serializer = new XmlSerializer(typeof(SoapEnvelopeResponse<TResponse>));
    //    var envelope = (SoapEnvelopeResponse<TResponse>)serializer.Deserialize(reader)!;

    //    return envelope.Body.Content;
    //}

    public static T DeserializeSoapResponse<T>(string xml, string customNamespace, string methodName) where T : class, ISoapBody
    {
        XmlSerializer envelopeSerializer = new XmlSerializer(typeof(SoapEnvelopeResponse));
        SoapEnvelopeResponse envelope;

        using (StringReader reader = new StringReader(xml))
        {
            envelope = (SoapEnvelopeResponse)envelopeSerializer.Deserialize(reader)!;
        }

        if (envelope.Body.Any == null || envelope.Body.Any.Length == 0)
        {
            throw new InvalidOperationException("The SOAP body is empty.");
        }

        // Assume the first element in the Body is the content we want
        XmlElement bodyContent = envelope.Body.Any[0];
        XmlSerializer bodySerializer = new XmlSerializer(typeof(T));

        using (StringReader reader = new StringReader(bodyContent.OuterXml))
        {
            return (T)bodySerializer.Deserialize(reader)!;
        }
    }

    //private static TResponse DeserializeSoapResponse<TResponse>(string soapResponse, string elementName, string customNamespace)
    //{
    //    if (string.IsNullOrWhiteSpace(elementName)) elementName = typeof(TResponse).Name;

    //    using var stringReader = new StringReader(soapResponse);
    //    using var xmlReader = new XmlTextReader(stringReader);
    //    // Move to the envelope element
    //    xmlReader.MoveToContent();
    //    xmlReader.ReadStartElement("Envelope", "http://schemas.xmlsoap.org/soap/envelope/");

    //    // Move to the body element
    //    xmlReader.ReadStartElement("Body", "http://schemas.xmlsoap.org/soap/envelope/");

    //    // Move to the response element
    //    xmlReader.ReadStartElement(elementName, customNamespace);

    //    // Deserialize the response element
    //    var xmlSerializer = new XmlSerializer(typeof(TResponse));
    //    var response = (TResponse)xmlSerializer.Deserialize(xmlReader)!;

    //    // Skip the end element of the response
    //    xmlReader.ReadEndElement();

    //    // Skip the end element of the body and envelope
    //    xmlReader.ReadEndElement();
    //    xmlReader.ReadEndElement();

    //    return response!;
    //}
}
