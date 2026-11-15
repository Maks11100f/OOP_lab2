namespace OOP_lab2;

public partial class App : Application
{
    /// <summary>Ключ збереженої теми: 0 — системна, 1 — світла, 2 — темна.</summary>
    public const string ThemeKey = "theme";

    public App()
    {
        InitializeComponent();
        ApplyTheme(Preferences.Default.Get(ThemeKey, 0));
    }

    /// <summary>
    /// Встановлює тему застосунку. Стилі (AppThemeBinding) і стандартні елементи
    /// Windows (Picker, RadioButton, Entry) перемикаються разом.
    /// </summary>
    public void ApplyTheme(int index)
    {
        UserAppTheme = index switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Unspecified   // як у Windows
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage())
        {
            Title = "ЛР2 — Розклад (робота з XML)",
            Width = 1280,
            Height = 820
        };
    }
}
