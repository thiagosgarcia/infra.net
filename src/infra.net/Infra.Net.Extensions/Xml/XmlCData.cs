using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Infra.Net.Extensions.Extensions;

namespace Infra.Net.Extensions.Xml
{
    public class XmlCData<TValue> : IXmlSerializable
    {
        public static implicit operator XmlCData<TValue>(TValue value)
        {
            return new XmlCData<TValue>(value);
        }

        public static implicit operator TValue(XmlCData<TValue> cData)
        {
            return cData.Value;
        }

        [XmlIgnore]
        public TValue Value { get; private set; }

        [XmlIgnore]
        public DynamicXml XmlValue { get; private set; }

        [XmlIgnore]
        public string LocalXmlRootAttribute { get; set; }

        public XmlCData() : this(default(TValue))
        {

        }

        public XmlCData(TValue value)
        {
            Value = value;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(XmlReader reader)
        {
            XmlValue = DynamicXml.Parse(reader);
            Value = XmlValue.ConvertTo<TValue>();
        }

        public void WriteXml(XmlWriter writer)
        {
            if ((typeof(TValue).IsValueType) ||
                (Value is string))
                writer.WriteCData(Value.ToString());
            else
                writer.WriteCData(Value.ToXml(false, true, LocalXmlRootAttribute));
        }
    }
}
