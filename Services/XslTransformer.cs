using System.Xml;
using System.Xml.Xsl;

namespace OOP_lab2.Services;

/// <summary>Трансформація XML → HTML на основі XSL-документа.</summary>
public class XslTransformer
{
    /// <summary>Трансформує XML-файл у HTML-файл.</summary>
    public void Transform(string xml, string xsl, string html)
    {
        var transform = Load(xsl);
        using var writer = XmlWriter.Create(html, transform.OutputSettings);
        transform.Transform(xml, writer);
    }

    /// <summary>Трансформує XML із довільного джерела у вже відкритий потік.</summary>
    public void Transform(XmlReader xml, string xsl, Stream output)
    {
        var transform = Load(xsl);
        using var writer = XmlWriter.Create(output, transform.OutputSettings);
        transform.Transform(xml, writer);
    }

    private static XslCompiledTransform Load(string xslPath)
    {
        var transform = new XslCompiledTransform();
        transform.Load(xslPath);
        return transform;
    }
}
