using OOP_lab2.Models;
using OOP_lab2.Services;

namespace OOP_lab2.Saving;

/// <summary>
/// Творець шаблону «Фабричний метод».
/// Базовий клас містить спільний код: відкриття файлу, запис і вивантаження на Google Drive.
/// Який саме об'єкт записуватиме дані у файл, вирішують підкласи у <see cref="CreateWriter"/>.
/// </summary>
public abstract class FragmentSaver
{
    protected readonly GoogleDriveService _drive;
    private readonly string _outputFolder;

    protected FragmentSaver(GoogleDriveService drive, string outputFolder)
    {
        _drive = drive;
        _outputFolder = outputFolder;
    }

    /// <summary>Розширення файлу разом з крапкою, напр. ".html".</summary>
    public abstract string Extension { get; }

    protected abstract string MimeType { get; }

    /// <summary>Фабричний метод.</summary>
    protected abstract IFragmentWriter CreateWriter();

    /// <summary>
    /// Зберігає фрагмент у локальний файл і вивантажує його на Google Drive.
    /// Повертає ідентифікатор файлу на Google Drive.
    /// </summary>
    public async Task<string> SaveAsync(IReadOnlyList<Lesson> lessons, string fileName)
    {
        if (!fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            fileName += Extension;

        Directory.CreateDirectory(_outputFolder);
        var localPath = Path.Combine(_outputFolder, fileName);

        // відкриття файлу — спільне для всіх форматів
        using (var stream = new FileStream(localPath, FileMode.Create, FileAccess.Write))
        {
            IFragmentWriter writer = CreateWriter();   // конкретний об'єкт обирає підклас
            writer.Write(stream, lessons);
        }

        return await _drive.UploadFileAsync(localPath, MimeType);
    }
}
