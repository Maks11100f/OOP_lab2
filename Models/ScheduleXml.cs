using System.Xml.Linq;

namespace OOP_lab2.Models;

/// <summary>
/// Опис структури XML-файлу розкладу:
/// &lt;Schedule&gt; → &lt;Lesson атрибути...&gt; → &lt;Students&gt; → &lt;Student Name="..."/&gt;
/// </summary>
public static class ScheduleXml
{
    // елементи
    public const string Root = "Schedule";
    public const string Lesson = "Lesson";
    public const string Students = "Students";
    public const string Student = "Student";
    public const string StudentName = "Name";

    // атрибути вузла Lesson
    public const string Teacher = "Teacher";
    public const string Faculty = "Faculty";
    public const string Department = "Department";
    public const string Room = "Room";
    public const string Subject = "Subject";
    public const string Group = "Group";
    public const string Day = "Day";
    public const string Time = "Time";

    /// <summary>
    /// Ключ пошуку за складом студентів. Студенти зберігаються не в атрибутах Lesson,
    /// а в дочірніх вузлах, тому для них використовується окремий ключ.
    /// </summary>
    public const string StudentKey = "Student";

    /// <summary>Назви атрибутів українською — для підписів у формі пошуку.</summary>
    public static string DisplayName(string key) => key switch
    {
        Teacher => "Викладач (П.І.П.)",
        Faculty => "Факультет",
        Department => "Кафедра",
        Room => "Аудиторія",
        Subject => "Дисципліна",
        Group => "Група",
        Day => "День",
        Time => "Час",
        StudentKey => "Студент",
        _ => key
    };

    /// <summary>Будує XML-документ тієї ж структури з відфільтрованих занять.</summary>
    public static XDocument ToDocument(IEnumerable<Models.Lesson> lessons)
    {
        var root = new XElement(Root);
        foreach (var l in lessons)
        {
            var e = new XElement(Lesson);
            AddAttr(e, Teacher, l.Teacher);
            AddAttr(e, Faculty, l.Faculty);
            AddAttr(e, Department, l.Department);
            AddAttr(e, Room, l.Room);
            AddAttr(e, Subject, l.Subject);
            AddAttr(e, Group, l.Group);
            AddAttr(e, Day, l.Day);
            AddAttr(e, Time, l.Time);
            e.Add(new XElement(Students,
                l.Students.Select(s => new XElement(Student, new XAttribute(StudentName, s)))));
            root.Add(e);
        }
        return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
    }

    private static void AddAttr(XElement e, string name, string? value)
    {
        if (value != null) e.SetAttributeValue(name, value);
    }
}
