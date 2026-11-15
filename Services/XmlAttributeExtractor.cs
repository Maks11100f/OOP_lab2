using System.Xml.Linq;
using OOP_lab2.Models;

namespace OOP_lab2.Services;

/// <summary>
/// «Підгрузка даних»: збирає всі атрибути вузлів Lesson і всі їхні значення,
/// що реально присутні у файлі. З них будуються списки вибору у формі пошуку.
/// </summary>
public class XmlAttributeExtractor
{
    public Dictionary<string, List<string>> GetAttributeValues(string xmlPath)
    {
        var doc = XDocument.Load(xmlPath);
        var values = new Dictionary<string, SortedSet<string>>();

        foreach (var lesson in doc.Descendants(ScheduleXml.Lesson))
        {
            foreach (var attr in lesson.Attributes())
                Add(values, attr.Name.LocalName, attr.Value);

            foreach (var student in lesson.Elements(ScheduleXml.Students).Elements(ScheduleXml.Student))
                Add(values, ScheduleXml.StudentKey, (string?)student.Attribute(ScheduleXml.StudentName));
        }

        // порядок ключів — як вони вперше зустрілися у файлі, значення — за абеткою
        return values.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
    }

    private static void Add(Dictionary<string, SortedSet<string>> values, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        if (!values.TryGetValue(key, out var set))
        {
            set = new SortedSet<string>(StringComparer.CurrentCulture);
            values[key] = set;
        }
        set.Add(value.Trim());
    }
}
