namespace OOP_lab2;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Інтерфейс розрахований на світлу тему. Без цього при темній темі Windows
        // стандартні елементи (Picker, RadioButton, Entry) малюють білий текст на світлому фоні.
        UserAppTheme = AppTheme.Light;
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
