namespace Infra.Net.Extensions.Xml;

public class DynamicXml : DynamicObject, IGenericDynamicObject
{
    private readonly XElement _root;
    private DynamicXml(XElement root)
    {
        _root = root;
    }

    public static DynamicXml Parse(Stream xmlReader)
    {
        xmlReader.Seek(0, SeekOrigin.Begin);
        var xDocument = XDocument.Load(xmlReader);
        return new DynamicXml(xDocument.Root);
    }

    public static DynamicXml Parse(XmlReader xmlReader)
    {
        var xDocument = XDocument.Load(xmlReader);
        return new DynamicXml(xDocument.Root);
    }

    public static DynamicXml Parse(TextReader xmlReader)
    {
        var xDocument = XDocument.Load(xmlReader);
        return new DynamicXml(xDocument.Root);
    }

    public static DynamicXml Parse(string xmlText)
    {
        var xDocument = XDocument.Parse(xmlText);
        return new DynamicXml(xDocument.Root);
    }

    public static DynamicXml Parse(XmlDocument xmlDocument)
    {
        var xmlReader = new XmlNodeReader(xmlDocument);
        var xDocument = XDocument.Load(xmlReader);

        xmlReader.Close();
        xmlReader.Dispose();

        return new DynamicXml(xDocument.Root);
    }

    public bool XmlIsEmpty { get { return _root.IsEmpty; } }
    public bool XmlHasElements { get { return _root.HasElements; } }
    public bool XmlHasAttributes { get { return _root.HasAttributes; } }

    public XElement XmlElement { get { return _root; } }

    public DynamicXml ParentElement
    {
        get { return (_root != null && _root.Parent != null) ? new DynamicXml(_root.Parent) : null; }
    }

    public dynamic AsDynamic { get { return this; } }

    IEnumerable<DynamicXml> _XmlElementsCollection;
    public IEnumerable<DynamicXml> XmlElementsCollection
    {
        get
        {
            if (_XmlElementsCollection == null)
                _XmlElementsCollection = _root.Elements().Select(n => new DynamicXml(n));
            return _XmlElementsCollection;
        }
    }

    IEnumerable<XAttribute> _XmlAttributes;
    public IEnumerable<XAttribute> XmlAttributes
    {
        get
        {
            if (_XmlAttributes == null)
                _XmlAttributes = _root.Attributes();
            return _XmlAttributes;
        }
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(_root.Value) ? _root.ToString() : _root.Value;
    }

    public dynamic XmlDataValue { get { return _root.Value; } }

    public string XmlData { get { return _root.ToString(); } }

    public IEnumerable<dynamic> XmlDataElements { get { return XmlElementsCollection; } }

    public override DynamicMetaObject GetMetaObject(System.Linq.Expressions.Expression parameter)
    {
        return new GenericDynamicMetaObject<DynamicXml>(parameter, this);
    }

    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
    {
        var dynamicIndexesValues = indexes.OfType<DynamicMetaObject>();
        var firstIndexValue = dynamicIndexesValues.Count() != 0 ?
            dynamicIndexesValues.Where(i => i != null && i.Value != null).Select(i => i.Value.ToString()).FirstOrDefault() :
            indexes.Where(i => i != null).Select(i => i.ToString()).FirstOrDefault();

        result = null;
        var idx = 0;
        var validIntIndex = int.TryParse(firstIndexValue, out idx);
        validIntIndex = validIntIndex && idx >= 0;

        if (!validIntIndex)
            result = GetValue(firstIndexValue);
        else if (XmlHasElements)
            result = XmlElementsCollection.Count() > idx ? XmlElementsCollection.ElementAt(idx) : null;
        else if (XmlHasAttributes)
            result = XmlAttributes.Count() > idx ? XmlAttributes.ElementAt(idx).Value : null;

        return true;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        result = GetValue(binder.Name);
        return true;
    }

    public DynamicXml GetElementByName(string elementName)
    {
        object result = null;
        result = GetValue(elementName, true);
        return result as DynamicXml;
    }

    public IEnumerable<DynamicXml> GetElementSetByName(string elementName)
    {
        object result = null;
        result = GetValue(elementName, false, true);
        return result as IEnumerable<DynamicXml>;
    }

    object GetValue(string binderPropertyName, bool forceElementForValueOnly = false, bool forceElementSet = false)
    {
        var att = _root.Attribute(binderPropertyName);
        if (att != null)
            return att.Value;

        var nodes = _root.Elements(binderPropertyName);
        if (nodes.Count() > 1)
            return nodes.Select(n => new DynamicXml(n));
        else if (!nodes.Any() && forceElementSet)
        {
            nodes = _root.DescendantNodes()
                .OfType<XElement>()
                .Where(xelement =>
                    xelement.Name.LocalName.Equals(binderPropertyName, StringComparison.InvariantCultureIgnoreCase));
            if (nodes.Count() != 0)
                return nodes.Select(n => new DynamicXml(n));
            return null;
        }

        var node = _root.Element(binderPropertyName)
            .IsNull(_root.Element(XName.Get(binderPropertyName, _root.Name.NamespaceName)))
            .IsNull(_root.Elements().FirstOrDefault(xml => xml.Name.LocalName == binderPropertyName));
        if (node != null)
        {
            if (node.HasElements || node.HasAttributes || forceElementForValueOnly)
                return new DynamicXml(node);
            else
                return node.Value;
        }

        var actualProperty = TypeDescriptor.GetProperties(this)
            .OfType<PropertyDescriptor>()
            .FirstOrDefault(p => p.Name == binderPropertyName);
        if (actualProperty != null)
            return actualProperty.GetValue(this);

        if (_root.Name.LocalName.Equals(binderPropertyName, StringComparison.InvariantCultureIgnoreCase))
            return _root.Value;

        return null;
    }

    public object GetDynamicMemberValue(string key)
    {
        return GetValue(key);
    }

    public object SetDynamicMember(string key, object value)
    {
        return value;
    }

    public object GetIndexValue(GetIndexBinder binder, object[] indexes)
    {
        object resultGetIndex = null;
        this.TryGetIndex(binder, indexes, out resultGetIndex);
        return resultGetIndex;
    }
}
