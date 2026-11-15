using OOP_lab2.Models;

namespace OOP_lab2.Strategies;

/// <summary>
/// Контекст шаблону «Стратегія»: зберігає обрану стратегію і делегує їй пошук.
/// Інтерфейс користувача не знає, як саме аналізується документ.
/// </summary>
public class XmlSearchContext
{
    private IXmlSearchStrategy _strategy;

    public XmlSearchContext(IXmlSearchStrategy? strategy = null)
    {
        _strategy = strategy ?? new SaxSearchStrategy();
    }

    public IXmlSearchStrategy Strategy => _strategy;

    public void SetStrategy(IXmlSearchStrategy strategy)
    {
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

    public List<Lesson> ExecuteSearch(string xmlPath, Dictionary<string, string> criteria)
    {
        return _strategy.Search(xmlPath, criteria);
    }
}
