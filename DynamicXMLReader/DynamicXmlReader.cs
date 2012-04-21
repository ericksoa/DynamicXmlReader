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

        internal DynamicXmlReader(XDocument xDocument)
        {
            _xElement = xDocument.Root;
        }

        internal DynamicXmlReader(XElement xElement)
        {
            _xElement = xElement;
        }

        public override bool TryInvokeMember(
            InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder.Name == "Using")
            {
                var path = (string) args[0];

                result = arrayOfDynamicsByContext(_xElement.XPathSelectElements(path));
                return true;
            }
            return base.TryInvokeMember(binder, args, out result);
        }

        private IEnumerable<T> append<T>(IEnumerable<T> source, T appendedItem)
        {
            yield return appendedItem;
            foreach (var item in source) yield return item;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            return tryBindMultipleMatchingElements(binder, indexes[0].ToString(), out result) ||
                   base.TryGetIndex(binder, indexes, out result);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return tryBindMultipleMatchingElements(binder, binder.Name, out result) ||
                   base.TryGetMember(binder, out result);
        }

        private bool tryBindMultipleMatchingElements(DynamicMetaObjectBinder binder, string name, out object result)
        {
            var matchingAttributes =
                from attribute in _xElement.Attributes()
                where
                    attribute.Name.LocalName.ToUpperInvariant()
                    == name.ToUpperInvariant()
                select attribute;
            var elements = append(_xElement.Elements(), _xElement);
            var matchingElements =
                from element in elements
                where element.Name.LocalName.ToUpperInvariant() == name.ToUpperInvariant()
                select element;

            if (tryBindToAttribute(matchingAttributes, out result)) return true;
            if (tryBindToValue(name, out result)) return true;

            if (matchingElements.Count() == 1)
                if (binder.ReturnType != typeof (IEnumerable<object>))
                    return tryBindSingleMatchingElement(matchingElements, out result);
                else
                    return tryBindMultipleMatchingElements(matchingElements, out result);
            if (matchingElements.Count() > 1)
                return tryBindMultipleMatchingElements(matchingElements, out result);

            return false;
        }

        private bool tryBindMultipleMatchingElements(IEnumerable<XElement> matchingElements, out object result)
        {
            result = arrayOfDynamicsByContext(matchingElements);
            return true;
        }

        private bool tryBindSingleMatchingElement(IEnumerable<XElement> matchingElements, out object result)
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

        private bool tryBindToAttribute(IEnumerable<XAttribute> matchingAttributes, out object result)
        {
            if (matchingAttributes.Any())
            {
                //return single result via attribute
                result = matchingAttributes.First().Value;
                return true;
            }
            result = null;
            return false;
        }

        private bool tryBindToValue(string name, out object result)
        {
            if (name == "Value")
            {
                result = _xElement.Value;
                return true;
            }
            result = null;
            return false;
        }

        private IEnumerable<dynamic> arrayOfDynamicsByContext(IEnumerable<XElement> elements)
        {
            return
                (
                    from element in elements
                    select
                        element.HasElements || element.HasAttributes
                            ? (dynamic) new DynamicXmlReader(element)
                            : (dynamic) element.Value
                ).ToArray();
        }
    }
}