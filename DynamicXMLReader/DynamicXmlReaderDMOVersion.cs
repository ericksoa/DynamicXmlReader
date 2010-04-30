using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.XPath;
using System.Xml.Linq;

namespace DynamicXMLReader
{
  //One day of work!
  public class DynamicXmlMetaObject : DynamicMetaObject
  {
    public DynamicXmlMetaObject(Expression expression, XNode node)
      : base(expression, BindingRestrictions.GetInstanceRestriction(expression, node), node)
    {}

    private DynamicXmlMetaObject(Expression expression, BindingRestrictions bindingRestrictions, XNode node)
      : base(expression, bindingRestrictions, node)
    {}

    public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
    {
      if (binder.Name == "Using")
      {
        var path = (string) args[0].Value;
        if (Value is XDocument)
        {
          var asDoc = Value as XDocument;
          return ArrayOfDynamicsByContext(asDoc.XPathSelectElements(path));
        }
        if (Value is XElement)
        {
          var asElem = Value as XElement;
          return ArrayOfDynamicsByContext(asElem.XPathSelectElements(path));
        }
      }
      throw new NotImplementedException();
    }

    public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
    {
      Func<XAttribute, bool> attributeNameMatchPredicate = a => String.Equals(a.Name.LocalName,binder.Name,StringComparison.CurrentCultureIgnoreCase);
      Func<XElement, bool> elementNameMatchPredicate = e => String.Equals(e.Name.LocalName,binder.Name,StringComparison.CurrentCultureIgnoreCase);

      if (binder.Name == "Value" && Value is XElement) 
        return StringForValue(Value as XElement);

      if (Value is XDocument)
      {
        var asDoc = Value as XDocument;
        return 
          asDoc.Descendants().Count(elementNameMatchPredicate) == 1 
            ? DynamicXmlReaderForSingleElement(asDoc.Descendants().Single(elementNameMatchPredicate)) 
            : ArrayOfDynamicsByContext(asDoc.Descendants().Where(elementNameMatchPredicate));
      }
      if (Value is XElement)
      {
        //if attribute, bind it to a string
        //if text with no children, bind it to a string
        //if neither, but it exists as an aggregate, bind it to the aggregate
        var asElem = Value as XElement;
        if(asElem.Attributes().Any(attributeNameMatchPredicate))
          return StringForFirstAttribute(asElem, attributeNameMatchPredicate);
        if (ElementContainsAtLeastOneMatchingElement(elementNameMatchPredicate, asElem))
        {
          var subElems = asElem.Elements().Where(elementNameMatchPredicate);
          if (subElems.Count() == 1)
            return 
              subElems.First().HasElements
                ? DynamicXmlReaderForSingleElement(subElems.First())
                : subElems.First().HasAttributes && binder.Name != "Value" 
                  ? DynamicXmlReaderForSingleElement(subElems.First()) 
                  : StringForValue(subElems.First());
          return ArrayOfDynamicsByContext(subElems);
        }
      }
      throw new NotImplementedException();
    }

    private static DynamicMetaObject ArrayOfDynamicsByContext(IEnumerable<XElement> subElems)
    {
      var xmlReaderArray = subElems.Select<XElement, object>(
        e => 
          e.HasElements || e.HasAttributes 
          ? (dynamic) new DynamicXmlReaderDmoVersion(e) 
          : (dynamic) e.Value 
      ).ToArray();
      var target = Expression.Constant(xmlReaderArray);
      return new DynamicMetaObject(target, BindingRestrictions.GetTypeRestriction(target, typeof(dynamic[])).Merge(BindingRestrictions.GetInstanceRestriction(target, xmlReaderArray)), xmlReaderArray);
    }

    private static DynamicMetaObject StringForValue(XElement subElem)
    {
      var value = subElem.Value;
      var target = Expression.Constant(value);
      return new DynamicMetaObject(target, BindingRestrictions.GetTypeRestriction(target, typeof(string)).Merge(BindingRestrictions.GetInstanceRestriction(target, value)), value);
    }

    private static bool ElementContainsAtLeastOneMatchingElement(Func<XElement, bool> elementNameMatchPredicate, XElement asElem)
    {
      return asElem.HasElements && asElem.Elements().Any(elementNameMatchPredicate);
    }

    private static DynamicMetaObject StringForFirstAttribute(XElement asElem, Func<XAttribute, bool> attributeNameMatchPredicate)
    {
      var value = asElem.Attributes().First(attributeNameMatchPredicate).Value;
      var target = Expression.Constant(value);
      return 
        new DynamicMetaObject(
          target,
          BindingRestrictions.GetTypeRestriction(target, typeof(string)).Merge(BindingRestrictions.GetInstanceRestriction(target, value)))
        ;
    }

    private static DynamicMetaObject DynamicXmlReaderForSingleElement(XElement elem)
    {
      var dynamicXmlReader = new DynamicXmlReaderDmoVersion(elem);
      var target = Expression.Constant(dynamicXmlReader);
      return new DynamicXmlMetaObject(target, BindingRestrictions.GetTypeRestriction(target, typeof(DynamicXmlReaderDmoVersion)).Merge(BindingRestrictions.GetInstanceRestriction(target, dynamicXmlReader)), elem);
    }
  }
  
  public class DynamicXmlReaderDmoVersion : IDynamicMetaObjectProvider
  {
    private readonly XNode _internalNode;

    internal DynamicXmlReaderDmoVersion(XNode node)
    {
      _internalNode = node;
    }

    public DynamicMetaObject GetMetaObject(Expression expression)
    {
      return new DynamicXmlMetaObject(expression, _internalNode);
    }

    public static dynamic Parse(string someXml)
    {
      var xdoc = XDocument.Parse(someXml);
      return new DynamicXmlReaderDmoVersion(xdoc);
    }

    public static dynamic Load(string someUrl)
    {
      var xdoc = XDocument.Load(someUrl);
      return new DynamicXmlReaderDmoVersion(xdoc);
    }
  }
}
