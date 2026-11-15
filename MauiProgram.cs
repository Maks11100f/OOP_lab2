#if WINDOWS
using Microsoft.Maui.LifecycleEvents;
#endif

namespace OOP_lab2;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

#if WINDOWS
        // Закриття вікна «хрестиком»: скасовуємо його і питаємо підтвердження.
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(windows => windows.OnWindowCreated(window =>
            {
                window.AppWindow.Closing += async (_, args) =>
                {
                    if (MainPage.ExitConfirmed) return;
                    args.Cancel = true;

                    if (Application.Current?.Windows.FirstOrDefault()?.Page is MainPage page)
                        await page.ConfirmExitAsync();
                };
            }));
        });
#endif

        return builder.Build();
    }
}
