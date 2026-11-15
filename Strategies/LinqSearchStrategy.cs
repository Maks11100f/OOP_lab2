using System.Xml.Linq;
using OOP_lab2.Models;

namespace OOP_lab2.Strategies;

/// <summary>
/// LINQ to XML: документ завантажується в XDocument, а запит складається
/// динамічно — до послідовності вузлів Lesson по черзі додаються умови Where
/// для кожного параметра, обраного користувачем.
/// </summary>
public class LinqSearchStrategy : IXmlSearchStrategy
{
    public string Name => "LINQ to XML";

    public List<Lesson> Search(string xmlPath, Dictionary<string, string> criteria)
    {
        var doc = XDocument.Load(xmlPath);
        IEnumerable<XElement> query = doc.Descendants(ScheduleXml.Lesson);

        foreach (var (key, value) in criteria)
        {
            query = key == ScheduleXml.StudentKey
                ? query.Where(l => StudentsOf(l).Contains(value))
                : query.Where(l => (string?)l.Attribute(key) == value);
        }

        return query
            .Select(l => Lesson.FromAttributes(key => (string?)l.Attribute(key), StudentsOf(l)))
            .ToList();
    }

    private static IEnumerable<string> StudentsOf(XElement lesson) =>
        lesson.Elements(ScheduleXml.Students)
              .Elements(ScheduleXml.Student)
              .Select(s => (string?)s.Attribute(ScheduleXml.StudentName))
              .Where(n => !string.IsNullOrWhiteSpace(n))
              .Select(n => n!);
}
