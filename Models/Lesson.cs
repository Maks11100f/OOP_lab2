namespace OOP_lab2.Models;

/// <summary>
/// Одне заняття з розкладу (вузол &lt;Lesson&gt; у XML-файлі).
/// Атрибути вузла можуть бути відсутні (наприклад, онлайн-заняття не має аудиторії),
/// тому всі текстові властивості допускають null.
/// </summary>
public class Lesson
{
    public string? Teacher { get; set; }      // П.І.П. викладача
    public string? Faculty { get; set; }      // факультет
    public string? Department { get; set; }   // кафедра викладача
    public string? Room { get; set; }         // аудиторія (null — онлайн)
    public string? Subject { get; set; }      // дисципліна навчального плану
    public string? Group { get; set; }        // академічна група
    public string? Day { get; set; }
    public string? Time { get; set; }
    public List<string> Students { get; set; } = new();

    /// <summary>Аудиторія для відображення в інтерфейсі.</summary>
    public string RoomText => Room is null ? "онлайн" : $"ауд. {Room}";

    /// <summary>Склад студентів одним рядком — для відображення в інтерфейсі.</summary>
    public string StudentsText => Students.Count == 0 ? "—" : string.Join(", ", Students);

    /// <summary>
    /// Створює заняття з атрибутів вузла. Кожна стратегія читає атрибути своїм API
    /// (XmlReader / XmlElement / XElement) і передає сюди лише функцію доступу до них.
    /// </summary>
    public static Lesson FromAttributes(Func<string, string?> attribute, IEnumerable<string> students)
    {
        return new Lesson
        {
            Teacher = attribute(ScheduleXml.Teacher),
            Faculty = attribute(ScheduleXml.Faculty),
            Department = attribute(ScheduleXml.Department),
            Room = attribute(ScheduleXml.Room),
            Subject = attribute(ScheduleXml.Subject),
            Group = attribute(ScheduleXml.Group),
            Day = attribute(ScheduleXml.Day),
            Time = attribute(ScheduleXml.Time),
            Students = students.ToList()
        };
    }

    public override string ToString() =>
        $"{Day} {Time} | {Subject} | {Teacher} | {Group} | ауд. {Room ?? "онлайн"}";
}
