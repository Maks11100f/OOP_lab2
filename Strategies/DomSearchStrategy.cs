using System.Text;
using System.Xml;
using OOP_lab2.Models;

namespace OOP_lab2.Strategies;

/// <summary>
/// DOM-підхід: документ повністю завантажується в дерево XmlDocument,
/// а запит до нього генерується динамічно у вигляді XPath-виразу, наприклад:
/// //Lesson[@Teacher='...' and Students/Student/@Name='...']
/// </summary>
public class DomSearchStrategy : IXmlSearchStrategy
{
    public string Name => "DOM API";

    public List<Lesson> Search(string xmlPath, Dictionary<string, string> criteria)
    {
        var doc = new XmlDocument();
        doc.Load(xmlPath);

        var result = new List<Lesson>();
        var nodes = doc.SelectNodes(BuildXPath(criteria));
        if (nodes == null) return result;

        foreach (XmlElement lesson in nodes.OfType<XmlElement>())
        {
            var students = lesson.SelectNodes($"{ScheduleXml.Students}/{ScheduleXml.Student}")!
                .OfType<XmlElement>()
                .Select(s => s.GetAttribute(ScheduleXml.StudentName))
                .Where(n => n.Length > 0);

            result.Add(Lesson.FromAttributes(
                key => lesson.HasAttribute(key) ? lesson.GetAttribute(key) : null, students));
        }
        return result;
    }

    /// <summary>Динамічна генерація XPath-запиту з обраних користувачем умов.</summary>
    public static string BuildXPath(Dictionary<string, string> criteria)
    {
        var conditions = criteria.Select(c => c.Key == ScheduleXml.StudentKey
            ? $"{ScheduleXml.Students}/{ScheduleXml.Student}/@{ScheduleXml.StudentName}={Literal(c.Value)}"
            : $"@{c.Key}={Literal(c.Value)}");

        var filter = string.Join(" and ", conditions);
        return filter.Length == 0 ? $"//{ScheduleXml.Lesson}" : $"//{ScheduleXml.Lesson}[{filter}]";
    }

    /// <summary>
    /// Рядковий літерал XPath 1.0. Значення можуть містити апостроф
    /// («Об'єктно-орієнтоване…»), тому в такому разі використовується concat().
    /// </summary>
    private static string Literal(string value)
    {
        if (!value.Contains('\'')) return $"'{value}'";
        if (!value.Contains('"')) return $"\"{value}\"";

        var sb = new StringBuilder("concat(");
        var parts = value.Split('\'');
        for (int i = 0; i < parts.Length; i++)
        {
            if (i > 0) sb.Append(", \"'\", ");
            sb.Append('\'').Append(parts[i]).Append('\'');
        }
        return sb.Append(')').ToString();
    }
}
