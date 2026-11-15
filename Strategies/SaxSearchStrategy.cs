using System.Xml;
using OOP_lab2.Models;

namespace OOP_lab2.Strategies;

/// <summary>
/// SAX-підхід: документ читається послідовно, вузол за вузлом (XmlReader),
/// без побудови дерева в пам'яті. Дані поточного заняття накопичуються,
/// а перевірка умов виконується на закриваючому тегі &lt;/Lesson&gt;.
/// </summary>
public class SaxSearchStrategy : IXmlSearchStrategy
{
    public string Name => "SAX API";

    public List<Lesson> Search(string xmlPath, Dictionary<string, string> criteria)
    {
        var result = new List<Lesson>();
        var settings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true,
            DtdProcessing = DtdProcessing.Prohibit
        };

        Dictionary<string, string>? attributes = null;
        List<string>? students = null;

        using var reader = XmlReader.Create(xmlPath, settings);
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == ScheduleXml.Lesson)
            {
                attributes = ReadAttributes(reader);
                students = new List<string>();
                if (reader.IsEmptyElement)          // <Lesson ... /> без дочірніх вузлів
                    Complete();
            }
            else if (reader.NodeType == XmlNodeType.Element && reader.Name == ScheduleXml.Student
                     && students != null)
            {
                var name = reader.GetAttribute(ScheduleXml.StudentName);
                if (!string.IsNullOrWhiteSpace(name))
                    students.Add(name);
            }
            else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == ScheduleXml.Lesson)
            {
                Complete();
            }
        }
        return result;

        void Complete()
        {
            if (attributes != null && students != null && Matches(attributes, students, criteria))
                result.Add(Lesson.FromAttributes(
                    key => attributes.TryGetValue(key, out var v) ? v : null, students));
            attributes = null;
            students = null;
        }
    }

    private static Dictionary<string, string> ReadAttributes(XmlReader reader)
    {
        var attributes = new Dictionary<string, string>();
        if (reader.MoveToFirstAttribute())
        {
            do attributes[reader.Name] = reader.Value;
            while (reader.MoveToNextAttribute());
            reader.MoveToElement();
        }
        return attributes;
    }

    private static bool Matches(Dictionary<string, string> attributes, List<string> students,
                                Dictionary<string, string> criteria)
    {
        foreach (var (key, value) in criteria)
        {
            bool ok = key == ScheduleXml.StudentKey
                ? students.Contains(value)
                : attributes.TryGetValue(key, out var actual) && actual == value;
            if (!ok) return false;
        }
        return true;
    }
}
