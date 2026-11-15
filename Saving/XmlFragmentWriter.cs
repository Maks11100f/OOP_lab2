using OOP_lab2.Models;

namespace OOP_lab2.Saving;

/// <summary>Записує фрагмент у форматі XML (та сама структура, що й у вхідному файлі).</summary>
public class XmlFragmentWriter : IFragmentWriter
{
    public void Write(Stream stream, IReadOnlyList<Lesson> lessons)
    {
        ScheduleXml.ToDocument(lessons).Save(stream);
    }
}
