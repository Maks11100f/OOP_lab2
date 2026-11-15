using OOP_lab2.Models;

namespace OOP_lab2.Strategies;

/// <summary>
/// Стратегія аналізу XML-документа (шаблон «Стратегія»).
/// criteria: назва атрибута → шукане значення; заняття має відповідати ВСІМ умовам.
/// Ключ <see cref="ScheduleXml.StudentKey"/> означає пошук за складом студентів.
/// </summary>
public interface IXmlSearchStrategy
{
    string Name { get; }

    List<Lesson> Search(string xmlPath, Dictionary<string, string> criteria);
}
