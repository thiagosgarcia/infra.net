namespace Infra.Net.Extensions.Xml;

public class XmlExtensionSettings
{
    public XmlExtensionSettings()
    {
        IsIdent = true;
        OmitXmlDeclaration = false;
        CodePageDefault = 65001;
        StandAlone = false;
    }

    public bool IsIdent { get; set; }

    public bool OmitXmlDeclaration { get; set; }

    public string XmlRootAttribute { get; set; }

    public int CodePageDefault { get; set; }

    public bool? StandAlone { get; set; }
}