using OOP_lab2.Models;
using OOP_lab2.Services;

namespace OOP_lab2.Saving;

/// <summary>
/// Записує фрагмент у форматі HTML: заняття спершу перетворюються на XML,
/// а потім трансформуються поточним XSL-документом.
/// </summary>
public class HtmlFragmentWriter : IFragmentWriter
{
    private readonly XslTransformer _transformer;
    private readonly string _xslPath;

    public HtmlFragmentWriter(XslTransformer transformer, string xslPath)
    {
        _transformer = transformer;
        _xslPath = xslPath;
    }

    public void Write(Stream stream, IReadOnlyList<Lesson> lessons)
    {
        using var xml = ScheduleXml.ToDocument(lessons).CreateReader();
        _transformer.Transform(xml, _xslPath, stream);
    }
}
