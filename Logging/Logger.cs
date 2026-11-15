using System.Text;

namespace OOP_lab2.Logging;

/// <summary>
/// Система протоколювання подій (шаблон «Одинак»).
/// Існує рівно один екземпляр на весь застосунок, доступний через <see cref="Instance"/>.
/// Журнал зберігається на локальному диску: Документи\OOP_lab2\log.txt
/// Формат запису: 21.09.2023 13:02:22 Фільтрація. Параметри: «Name» – aaa
/// </summary>
public sealed class Logger
{
    private static Logger? _instance;
    private static readonly object _lock = new();

    private readonly string _logFilePath;

    // приватний конструктор — створити об'єкт ззовні неможливо
    private Logger()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OOP_lab2");
        Directory.CreateDirectory(folder);
        _logFilePath = Path.Combine(folder, "log.txt");
    }

    public static Logger Instance
    {
        get
        {
            if (_instance == null)              // перша перевірка — без блокування
            {
                lock (_lock)
                {
                    _instance ??= new Logger(); // друга перевірка — під блокуванням
                }
            }
            return _instance;
        }
    }

    public string LogFilePath => _logFilePath;

    public void Log(LogLevel level, string message)
    {
        var line = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} {LevelName(level)}. {message}";
        lock (_lock)
        {
            File.AppendAllText(_logFilePath, line + Environment.NewLine, Encoding.UTF8);
        }
    }

    private static string LevelName(LogLevel level) => level switch
    {
        LogLevel.Filtration => "Фільтрація",
        LogLevel.Transformation => "Трансформація",
        LogLevel.Saving => "Збереження",
        _ => level.ToString()
    };
}
