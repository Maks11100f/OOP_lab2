using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace OOP_lab2.Services;

/// <summary>
/// Робота з Google Drive: перелік, завантаження та вивантаження файлів.
/// Для авторизації потрібен OAuth-клієнт типу «Desktop app» — файл
/// Resources/Raw/credentials.json (див. README.md). При першому зверненні
/// відкривається браузер для входу в Google-акаунт, токен кешується локально.
/// </summary>
public class GoogleDriveService
{
    private const string CredentialsFile = "credentials.json";
    private const string ApplicationName = "OOP_lab2";

    private DriveService? _service;

    /// <summary>Файли на Google Drive із заданими розширеннями: id → ім'я.</summary>
    public async Task<Dictionary<string, string>> ListFilesAsync(params string[] extensions)
    {
        var service = await GetServiceAsync();
        var files = new Dictionary<string, string>();
        string? pageToken = null;

        do
        {
            var request = service.Files.List();
            request.Q = "trashed = false and mimeType != 'application/vnd.google-apps.folder'";
            request.Fields = "nextPageToken, files(id, name)";
            request.OrderBy = "name";
            request.PageSize = 200;
            request.PageToken = pageToken;

            var page = await request.ExecuteAsync();
            foreach (var file in page.Files ?? new List<DriveFile>())
            {
                if (extensions.Any(e => file.Name.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
                    files[file.Id] = file.Name;
            }
            pageToken = page.NextPageToken;
        }
        while (pageToken != null && files.Count < 500);

        return files;
    }

    public async Task DownloadFileAsync(string fileId, string localPath)
    {
        var service = await GetServiceAsync();
        var request = service.Files.Get(fileId);

        using var stream = new FileStream(localPath, FileMode.Create, FileAccess.Write);
        var progress = await request.DownloadAsync(stream);
        if (progress.Status != DownloadStatus.Completed)
            throw new IOException("Не вдалося завантажити файл з Google Drive.", progress.Exception);
    }

    /// <summary>Вивантажує локальний файл на Google Drive і повертає його id.</summary>
    public async Task<string> UploadFileAsync(string localPath, string mimeType)
    {
        var service = await GetServiceAsync();
        var metadata = new DriveFile { Name = Path.GetFileName(localPath) };

        using var stream = File.OpenRead(localPath);
        var request = service.Files.Create(metadata, stream, mimeType);
        request.Fields = "id, name";

        var progress = await request.UploadAsync();
        if (progress.Status != UploadStatus.Completed)
            throw new IOException("Не вдалося вивантажити файл на Google Drive.", progress.Exception);

        return request.ResponseBody.Id;
    }

    private async Task<DriveService> GetServiceAsync()
    {
        if (_service != null) return _service;

        Stream credentials;
        try
        {
            credentials = await FileSystem.OpenAppPackageFileAsync(CredentialsFile);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Не знайдено Resources/Raw/credentials.json. Створіть OAuth-клієнт " +
                "«Desktop app» у Google Cloud Console і покладіть файл у проєкт (див. README.md).", ex);
        }

        using (credentials)
        {
            var secrets = GoogleClientSecrets.FromStream(credentials).Secrets;
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                new[] { DriveService.Scope.Drive },
                "user",
                CancellationToken.None,
                new FileDataStore(Path.Combine(FileSystem.AppDataDirectory, "google-token"), true));

            _service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
        }
        return _service;
    }
}
