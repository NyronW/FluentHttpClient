using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Concurrent;
using System.Reflection;

namespace FluentHttpClient.SoapMessaging;

public static class ISoapMessagingExtensions
{
    private static readonly ConcurrentDictionary<Type, XmlSerializer> serializers = new ConcurrentDictionary<Type, XmlSerializer>();

    private static XmlSerializer GetOrCreateSerializer(Type type)
    {
        return serializers.GetOrAdd(type, t => new XmlSerializer(t));
    }


    public static async Task<HttpResponseMessage> SoapPostAsync(this ISendRequestWithBody client, string soapPayload)
    {
        HttpContent content = new StringContent(soapPayload, Encoding.UTF8, "text/xml");

        return await ((ISetContentType)client).UsingContentType("text/xml")
            .PostAsync(content);
    }

    public static async Task<HttpResponseMessage> SoapPostAsync<TRequest>(this ISendRequestWithBody client, TRequest request,
        string methodName = "", string customNamespace = "")
    {
        HttpContent content = BuildSoapContent(request, methodName, customNamespace);

        return await ((ISetContentType)client).UsingContentType("text/xml")
            .PostAsync(content);
    }

    public static async Task<TResponse> SoapPostAsync<TRequest, TResponse>(this ISendRequestWithBody client, TRequest request, string methodName = "",
        string customNamespace = "") where TResponse : class
    {
        var response = await client.SoapPostAsync(request, methodName, customNamespace);
        string result = await response.Content.ReadAsStringAsync();

        var responseObject = DeserializeSoapResponse<TResponse>(result);

        return responseObject;
    }


    private static StringContent BuildSoapContent<TRequest>(TRequest request, string methodName, string customNamespace)
    {
        // Create a new XML document and writer
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { OmitXmlDeclaration = true });
        // Start writing the SOAP envelope
        xmlWriter.WriteStartElement("soap", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
        xmlWriter.WriteStartElement("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/");

        // Start writing the method element
        if (!string.IsNullOrWhiteSpace(methodName) && !string.IsNullOrEmpty(customNamespace))
            xmlWriter.WriteStartElement(methodName, customNamespace);

        // Check if the type has the XmlRoot attribute
        var xmlRootAttr = request!.GetType().GetCustomAttribute<XmlRootAttribute>();
        if (xmlRootAttr != null)
        {
            // Use XmlSerializer to serialize the body content
            var bodySerializer = GetOrCreateSerializer(typeof(TRequest));
            bodySerializer.Serialize(xmlWriter, request, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
        }
        else
        {
            // Use custom serialization for properties
            SerializeProperties(xmlWriter, request);
        }

        // Close the method, body, and envelope elements
        if (!string.IsNullOrWhiteSpace(methodName) && !string.IsNullOrEmpty(customNamespace))
            xmlWriter.WriteEndElement(); // method element
        else if (xmlRootAttr != null)
        {
            customNamespace = xmlRootAttr.Namespace!;
            methodName = xmlRootAttr.ElementName;
        }

        xmlWriter.WriteEndElement(); // body element
        xmlWriter.WriteEndElement(); // envelope element
        xmlWriter.Flush();

        if (customNamespace.EndsWith('/')) customNamespace = customNamespace.Substring(0, customNamespace.Length - 1);

        if (string.IsNullOrWhiteSpace(methodName) || string.IsNullOrEmpty(customNamespace))
            throw new ArgumentException("Cannot set SoapAction header as the namespace and/or method is not vaild");

        var xml = stringWriter.ToString();

        var content = new StringContent(xml, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", $"{customNamespace}/{methodName}");
        return content;
    }

    private static void SerializeProperties<T>(XmlWriter writer, T request)
    {
        if (request == null) return;

        Type type = request.GetType();
        foreach (PropertyInfo prop in type.GetProperties())
        {
            var value = prop.GetValue(request);
            if (value != null)
            {
                writer.WriteStartElement(prop.Name);

                if (IsSimpleType(prop.PropertyType))
                {
                    writer.WriteValue(value);
                }
                else
                {
                    SerializeProperties(writer, value);
                }

                writer.WriteEndElement();
            }
        }
    }

    private static bool IsSimpleType(Type type)
    {
        return
            type.IsPrimitive ||
            type.IsEnum ||
            type == typeof(string) ||
            type == typeof(decimal) ||
            type == typeof(DateTime) ||
            type == typeof(Guid);
    }


    public static T DeserializeSoapResponse<T>(string soapResponse) where T : class
    {
        SoapEnvelopeResponse envelope;

        using (StringReader reader = new(soapResponse))
        {
            XmlSerializer envelopeSerializer = new(typeof(SoapEnvelopeResponse));
            envelope = (SoapEnvelopeResponse)envelopeSerializer.Deserialize(reader)!;
        }

        if (envelope is not { Body.Any.Length: > 0 })
        {
            throw new InvalidOperationException("The SOAP body is empty.");
        }

        // Assume the first element in the Body is the content we want
        XmlElement bodyContent = envelope.Body.Any[0];

        using (StringReader reader = new(bodyContent.OuterXml))
        {
            XmlSerializer bodySerializer = new(typeof(T));
            return (T)bodySerializer.Deserialize(reader)!;
        }
    }
}
