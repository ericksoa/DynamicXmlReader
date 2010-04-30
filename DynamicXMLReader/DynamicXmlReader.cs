using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DynamicXMLReader
{
  public class DynamicXmlReader : DynamicObject
  {
    private readonly XElement _xElement;

    private DynamicXmlReader(XDocument xDocument)
    {
      _xElement = xDocument.Root;
    }

    private DynamicXmlReader(XElement xElement)
    {
      _xElement = xElement; 
    }

    public static dynamic Parse(string someXml)
    {
      return new DynamicXmlReader(XDocument.Parse(someXml));
    }

    public static dynamic Load(string someUrl)
    {
      return new DynamicXmlReader(XDocument.Load(someUrl));
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
      if (binder.Name == "Using")
      {
        var path = (string)args[0];
        
        result = ArrayOfDynamicsByContext(_xElement.XPathSelectElements(path));
        return true;
      }
      return base.TryInvokeMember(binder, args, out result);
    }


    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
      var matchingAttributes =
        (from attribute in _xElement.Attributes() where attribute.Name.LocalName.ToUpperInvariant() == binder.Name.ToUpperInvariant() select attribute)
          .ToList();
      var elements = _xElement.Elements().ToList();
      elements.Add(_xElement);
      var matchingElements =
        (from element in elements where element.Name.LocalName.ToUpperInvariant() == binder.Name.ToUpperInvariant() select element)
          .ToList();

      if (TryBindToAttribute(matchingAttributes, out result)) return true;
      if (TryBindToValue(binder, out result)) return true;
      
      if (matchingElements.Count() == 1)
        if (binder.ReturnType != typeof(IEnumerable<object>))
          return TryBindSingleMatchingElement(matchingElements, out result);
        else
          return TryBindMultipleMatchingElements(matchingElements, out result);
      if (matchingElements.Count() > 1) return TryBindMultipleMatchingElements(matchingElements, out result);

      return base.TryGetMember(binder, out result);
    }

    private static bool TryBindMultipleMatchingElements(IEnumerable<XElement> matchingElements, out object result)
    {
      result = ArrayOfDynamicsByContext(matchingElements);
      return true;
    }

    private static bool TryBindSingleMatchingElement(IEnumerable<XElement> matchingElements, out object result)
    {
      var matchingElement = matchingElements.First();
      if (matchingElement.HasElements || matchingElement.HasAttributes)
      {
        result = new DynamicXmlReader(matchingElement);
        return true;
      }
      result = matchingElement.Value;
      return true;
    }

    private static bool TryBindToAttribute(IEnumerable<XAttribute> matchingAttributes, out object result)
    {
      if (matchingAttributes.Count() > 0)
      {
        //return single result via attribute
        result = matchingAttributes.First().Value;
        return true;
      }
      result = null;
      return false;
    }

    private bool TryBindToValue(GetMemberBinder binder, out object result)
    {
      if (binder.Name == "Value")
      {
        result = _xElement.Value;
        return true;
      }
      result = null;
      return false;
    }

    private static dynamic[] ArrayOfDynamicsByContext(IEnumerable<XElement> elements)
    {
      return
        (
          from element in elements
          select
            element.HasElements || element.HasAttributes
              ? (dynamic)new DynamicXmlReader(element)
              : (dynamic)element.Value
        ).ToArray();
    }
  }
}