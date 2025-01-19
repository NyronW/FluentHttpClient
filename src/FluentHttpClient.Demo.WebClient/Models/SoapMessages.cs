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
public class AddResponse : ISoapResponseBody
{
    [XmlElement]
    public int AddResult { get; set; }
}

[XmlRoot("MultiplyResponse", Namespace = "http://tempuri.org/")]
public class MultiplyResponse : ISoapResponseBody
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

public class TContinent
{
    [XmlElement("sCode")]
    public string SCode { get; set; }
    [XmlElement("sName")]
    public string SName { get; set; }
}

public class ListOfContinentsByNameResult
{
    [XmlElement("tContinent")]
    public List<TContinent> TContinent { get; set; }
}

[XmlRoot(ElementName = "ListOfContinentsByNameResponse", Namespace = "http://www.oorsprong.org/websamples.countryinfo")]
public class ListOfContinentsByNameResponse: ISoapResponseBody
{
    [XmlElement("ListOfContinentsByNameResult")]
    public ListOfContinentsByNameResult ListOfContinentsByNameResult { get; set; }
}
