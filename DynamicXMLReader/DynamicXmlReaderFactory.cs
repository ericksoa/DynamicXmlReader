using System.Xml.Linq;
using DynamicXMLReader;

public interface IDynamicXmlReaderFactory
{
    dynamic Parse(string someXml);
    dynamic Load(string someUrl);
}

public class DynamicXmlReaderFactory : IDynamicXmlReaderFactory
{
    public dynamic Parse(string someXml)
    {
        return new DynamicXmlReader(XDocument.Parse(someXml));
    }

    public dynamic Load(string someUrl)
    {
        return new DynamicXmlReader(XDocument.Load(someUrl));
    }
}