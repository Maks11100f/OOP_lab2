using OOP_lab2.Logging;
using OOP_lab2.Models;
using OOP_lab2.Saving;
using OOP_lab2.Services;
using OOP_lab2.Strategies;
using LogLevel = OOP_lab2.Logging.LogLevel;

namespace OOP_lab2;

public partial class MainPage : ContentPage
{
    private const string AnyValue = "— будь-яке —";

    private readonly XmlSearchContext _searchContext = new();
    private readonly XmlAttributeExtractor _extractor = new();
    private readonly XslTransformer _transformer = new();
    private readonly GoogleDriveService _drive = new();

    /// <summary>Динамічно створені списки вибору: назва атрибута → Picker.</summary>
    private readonly Dictionary<string, Picker> _filterPickers = new();

    /// <summary>Сюди зберігаються HTML/XML-файли (Документи\OOP_lab2).</summary>
    private readonly string _outputFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OOP_lab2");

    private string? _xmlPath;
    private string? _xslPath;
    private List<Lesson> _results = new();
    private bool _exitDialogOpen;

    /// <summary>Користувач підтвердив вихід — вікно можна закривати.</summary>
    public static bool ExitConfirmed { get; private set; }

    public MainPage()
    {
        InitializeComponent();
        ThemePicker.SelectedIndex = Preferences.Default.Get(App.ThemeKey, 0);
        Loaded += async (_, _) => await LoadDefaultFilesAsync();
    }

    // =====================================================================
    //  Завантаження файлів
    // =====================================================================

    /// <summary>Приклад schedule.xml + schedule.xsl з ресурсів застосунку.</summary>
    private async Task LoadDefaultFilesAsync()
    {
        try
        {
            var folder = Path.Combine(FileSystem.AppDataDirectory, "samples");
            Directory.CreateDirectory(folder);
            SetXsl(await CopyPackageFileAsync("schedule.xsl", folder));
            await LoadXmlAsync(await CopyPackageFileAsync("schedule.xml", folder));
            SetStatus($"Журнал подій: {Logger.Instance.LogFilePath}");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Не вдалося завантажити файли-приклади", ex);
        }
    }

    private static async Task<string> CopyPackageFileAsync(string name, string folder)
    {
        var path = Path.Combine(folder, name);
        using var source = await FileSystem.OpenAppPackageFileAsync(name);
        using var target = File.Create(path);
        await source.CopyToAsync(target);
        return path;
    }

    private async void OnOpenFileClicked(object? sender, EventArgs e)
    {
        var path = await PickLocalFileAsync("Оберіть XML-файл", ".xml");
        if (path != null) await LoadXmlAsync(path);
    }

    private async void OnOpenFromDriveClicked(object? sender, EventArgs e)
    {
        var path = await PickFromDriveAsync("XML-файл з Google Drive", ".xml");
        if (path != null) await LoadXmlAsync(path);
    }

    private async void OnOpenXslClicked(object? sender, EventArgs e)
    {
        var path = await PickLocalFileAsync("Оберіть XSL-файл", ".xsl", ".xslt");
        if (path != null) SetXsl(path);
    }

    private async void OnOpenXslFromDriveClicked(object? sender, EventArgs e)
    {
        var path = await PickFromDriveAsync("XSL-файл з Google Drive", ".xsl", ".xslt");
        if (path != null) SetXsl(path);
    }

    /// <summary>«Підгрузка даних»: атрибути файлу → списки вибору у формі пошуку.</summary>
    private async Task LoadXmlAsync(string path)
    {
        Dictionary<string, List<string>> values;
        try
        {
            values = _extractor.GetAttributeValues(path);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Не вдалося прочитати XML-файл", ex);
            return;
        }

        _xmlPath = path;
        XmlFileLabel.Text = Path.GetFileName(path);
        BuildFilters(values);
        ClearResults();
        SetStatus($"Завантажено {Path.GetFileName(path)}: атрибутів для пошуку — {values.Count}");
    }

    private void SetXsl(string path)
    {
        _xslPath = path;
        XslFileLabel.Text = Path.GetFileName(path);
    }

    private void BuildFilters(Dictionary<string, List<string>> values)
    {
        FiltersPanel.Children.Clear();
        _filterPickers.Clear();

        foreach (var (key, list) in values)
        {
            var picker = new Picker { FontSize = 13 };
            picker.Items.Add(AnyValue);
            foreach (var value in list)
                picker.Items.Add(value);
            picker.SelectedIndex = 0;

            var row = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(new GridLength(120)),
                    new ColumnDefinition(GridLength.Star)
                },
                ColumnSpacing = 8
            };
            row.Add(new Label
            {
                Text = ScheduleXml.DisplayName(key),
                FontSize = 13,
                VerticalOptions = LayoutOptions.Center
            }, 0, 0);
            row.Add(picker, 1, 0);

            FiltersPanel.Children.Add(row);
            _filterPickers[key] = picker;
        }
    }

    // =====================================================================
    //  Пошук (шаблон «Стратегія»)
    // =====================================================================

    private async void OnSearchClicked(object? sender, EventArgs e)
    {
        if (_xmlPath == null)
        {
            await DisplayAlert("Пошук", "Спершу оберіть XML-файл.", "OK");
            return;
        }

        var criteria = GetCriteria();
        if (criteria.Count == 0)
        {
            await DisplayAlert("Пошук", "Оберіть значення хоча б одного параметра.", "OK");
            return;
        }

        IXmlSearchStrategy strategy =
            DomRadio.IsChecked ? new DomSearchStrategy() :
            LinqRadio.IsChecked ? new LinqSearchStrategy() :
            new SaxSearchStrategy();
        _searchContext.SetStrategy(strategy);

        try
        {
            _results = _searchContext.ExecuteSearch(_xmlPath, criteria);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Помилка аналізу документа", ex);
            return;
        }

        ResultsView.ItemsSource = _results;
        ResultsHeader.Text = $"Знайдено занять: {_results.Count}  ({strategy.Name})";

        var parameters = string.Join(", ", criteria.Select(c => $"«{c.Key}» – {c.Value}"));
        Logger.Instance.Log(LogLevel.Filtration,
            $"Параметри: {parameters}. Спосіб: {strategy.Name}. Знайдено: {_results.Count}");
        SetStatus($"Пошук виконано: {strategy.Name}");
    }

    private Dictionary<string, string> GetCriteria()
    {
        var criteria = new Dictionary<string, string>();
        foreach (var (key, picker) in _filterPickers)
        {
            if (picker.SelectedIndex > 0)
                criteria[key] = picker.Items[picker.SelectedIndex];
        }
        return criteria;
    }

    // =====================================================================
    //  Трансформація XML → HTML (XSLT)
    // =====================================================================

    private async void OnTransformClicked(object? sender, EventArgs e)
    {
        if (_xmlPath == null || _xslPath == null)
        {
            await DisplayAlert("Трансформація", "Потрібні XML- та XSL-файли.", "OK");
            return;
        }

        var fileName = BaseFileName() + ".html";
        var htmlPath = Path.Combine(_outputFolder, fileName);
        string what;

        try
        {
            Directory.CreateDirectory(_outputFolder);
            if (_results.Count > 0)
            {
                // є результати пошуку — трансформується відфільтрований фрагмент
                using var output = File.Create(htmlPath);
                using var xml = ScheduleXml.ToDocument(_results).CreateReader();
                _transformer.Transform(xml, _xslPath, output);
                what = "Відфільтрований фрагмент";
            }
            else
            {
                _transformer.Transform(_xmlPath, _xslPath, htmlPath);
                what = "Весь документ";
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Помилка трансформації", ex);
            return;
        }

        Logger.Instance.Log(LogLevel.Transformation, $"Збережено у файл {fileName}");
        SetStatus($"HTML: {htmlPath}");

        bool open = await DisplayAlert("Трансформація",
            $"{what} трансформовано у файл:\n{htmlPath}\n\nВідкрити його?", "Так", "Ні");
        if (open)
            await Launcher.Default.OpenAsync(new OpenFileRequest(fileName, new ReadOnlyFile(htmlPath)));
    }

    // =====================================================================
    //  Збереження фрагмента на Google Drive (шаблон «Фабричний метод»)
    // =====================================================================

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var format = (sender as Button)?.CommandParameter as string ?? "xml";

        if (_results.Count == 0)
        {
            await DisplayAlert("Збереження",
                "Спершу виконайте пошук — зберігається відфільтрований фрагмент.", "OK");
            return;
        }
        if (format == "html" && _xslPath == null)
        {
            await DisplayAlert("Збереження", "Для HTML потрібен XSL-файл.", "OK");
            return;
        }

        FragmentSaver saver = CreateSaver(format);
        var fileName = BaseFileName() + saver.Extension;

        SetBusy(true, "Вивантаження на Google Drive…");
        try
        {
            var fileId = await saver.SaveAsync(_results, fileName);
            Logger.Instance.Log(LogLevel.Saving, $"Збережено відфільтрований фрагмент у файл {fileName}");
            SetStatus($"Збережено на Google Drive: {fileName}");
            await DisplayAlert("Google Drive", $"Файл {fileName} збережено на Google Drive.\nId: {fileId}", "OK");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Не вдалося зберегти файл на Google Drive", ex);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Обирає конкретного «творця» — саме він вирішить, який об'єкт запише файл.</summary>
    private FragmentSaver CreateSaver(string format) => format == "html"
        ? new HtmlFragmentSaver(_drive, _outputFolder, _transformer, _xslPath!)
        : new XmlFragmentSaver(_drive, _outputFolder);

    private string BaseFileName()
    {
        var name = Path.GetFileNameWithoutExtension(FileNameEntry.Text?.Trim() ?? "");
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c.ToString(), "");
        return string.IsNullOrWhiteSpace(name) ? "fragment" : name;
    }

    // =====================================================================
    //  Google Drive: вибір файлу
    // =====================================================================

    private async Task<string?> PickFromDriveAsync(string title, params string[] extensions)
    {
        Dictionary<string, string> files;
        SetBusy(true, "Підключення до Google Drive…");
        try
        {
            files = await _drive.ListFilesAsync(extensions);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Помилка Google Drive", ex);
            return null;
        }
        finally
        {
            SetBusy(false);
        }

        if (files.Count == 0)
        {
            await DisplayAlert(title, $"На Google Drive немає файлів {string.Join(" / ", extensions)}.", "OK");
            return null;
        }

        // однакові імена файлів на Drive можливі — робимо підписи унікальними
        var byLabel = new Dictionary<string, string>();
        foreach (var (id, name) in files)
        {
            var label = name;
            for (int n = 2; byLabel.ContainsKey(label); n++)
                label = $"{name} ({n})";
            byLabel[label] = id;
        }

        const string cancel = "Скасувати";
        var choice = await DisplayActionSheet(title, cancel, null, byLabel.Keys.ToArray());
        if (choice == null || choice == cancel || !byLabel.TryGetValue(choice, out var fileId))
            return null;

        var folder = Path.Combine(FileSystem.AppDataDirectory, "drive");
        Directory.CreateDirectory(folder);
        var localPath = Path.Combine(folder, files[fileId]);

        SetBusy(true, "Завантаження файлу з Google Drive…");
        try
        {
            await _drive.DownloadFileAsync(fileId, localPath);
            return localPath;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Не вдалося завантажити файл", ex);
            return null;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task<string?> PickLocalFileAsync(string title, params string[] extensions)
    {
        var fileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, extensions }
        });

        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = title,
                FileTypes = fileTypes
            });
            return result?.FullPath;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Не вдалося відкрити файл", ex);
            return null;
        }
    }

    // =====================================================================
    //  Clear та вихід
    // =====================================================================

    private void OnClearClicked(object? sender, EventArgs e)
    {
        foreach (var picker in _filterPickers.Values)
            picker.SelectedIndex = 0;
        SaxRadio.IsChecked = true;
        ClearResults();
        SetStatus("Поле виводу та параметри пошуку очищено");
    }

    private void ClearResults()
    {
        _results = new List<Lesson>();
        ResultsView.ItemsSource = null;
        ResultsHeader.Text = "Результати пошуку";
    }

    private async void OnExitClicked(object? sender, EventArgs e) => await ConfirmExitAsync();

    /// <summary>Викликається кнопкою «Вихід» і при закритті вікна (див. MauiProgram).</summary>
    public async Task ConfirmExitAsync()
    {
        if (_exitDialogOpen) return;
        _exitDialogOpen = true;
        try
        {
            bool exit = await DisplayAlert("Вихід",
                "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");
            if (!exit) return;

            ExitConfirmed = true;
            Application.Current?.Quit();
        }
        finally
        {
            _exitDialogOpen = false;
        }
    }

    // =====================================================================
    //  Тема оформлення
    // =====================================================================

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        var index = ThemePicker.SelectedIndex;
        if (index < 0) return;

        (Application.Current as App)?.ApplyTheme(index);
        Preferences.Default.Set(App.ThemeKey, index);   // запам'ятовуємо вибір між запусками
    }

    // =====================================================================
    //  Допоміжне
    // =====================================================================

    private void SetStatus(string text) => StatusLabel.Text = text;

    private void SetBusy(bool busy, string? text = null)
    {
        BusyIndicator.IsVisible = busy;
        BusyIndicator.IsRunning = busy;
        if (text != null) SetStatus(text);
    }

    private Task ShowErrorAsync(string title, Exception ex)
    {
        var message = ex.InnerException == null
            ? ex.Message
            : $"{ex.Message}\n\n{ex.InnerException.Message}";
        SetStatus(title);
        return DisplayAlert(title, message, "OK");
    }
}
