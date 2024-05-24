using FluentHttpClient.SoapMessaging;
using System.Xml.Serialization;

namespace FluentHttpClient.Demo.WebClient.Models;

public class SoapCalculatorRequest
{
    public SoapCalculatorRequest()
    {

    }

    public SoapCalculatorRequest(int inta, int intb)
    {
        intA = inta;
        intB = intb;
    }

    public int intA { get; set; }
    public int intB { get; set; }
}

//[XmlRoot("Add", Namespace = "http://tempuri.org/")]
public class AddRequest : SoapCalculatorRequest
{
    public AddRequest() { }
    public AddRequest(int inta, int intb) : base(inta, intb)
    {
    }
}

[XmlRoot("Multiply", Namespace = "http://tempuri.org/")]
public class MultiplyRequest : SoapCalculatorRequest
{
    public MultiplyRequest() { }
    public MultiplyRequest(int inta, int intb) : base(inta, intb)
    {
    }
}

[XmlRoot("AddResponse", Namespace = "http://tempuri.org/")]
public class AddResponse : ISoapBody
{
    [XmlElement]
    public int AddResult { get; set; }
}

[XmlRoot("MultiplyResponse", Namespace = "http://tempuri.org/")]
public class MultiplyResponse : ISoapBody
{
    [XmlElement]
    public int MultiplyResult { get; set; }
}

public class SoapViewModel
{
    public int intA { get; set; }
    public int intB { get; set; }
    public string SoapServiceUrl { get; set; }
    public int AdditionResult { get; set; }
    public int MultiplicationResult { get; set; }
}

[XmlRoot("ListOfContinentsByName", Namespace = "http://www.oorsprong.org/websamples.countryinfo")]
public class ListOfContinents
{

}