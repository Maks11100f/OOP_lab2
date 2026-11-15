namespace OOP_lab2;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
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
