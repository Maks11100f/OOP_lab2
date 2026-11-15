using OOP_lab2.Services;

namespace OOP_lab2.Saving;

public class HtmlFragmentSaver : FragmentSaver
{
    private readonly XslTransformer _transformer;
    private readonly string _xslPath;

    public HtmlFragmentSaver(GoogleDriveService drive, string outputFolder,
                             XslTransformer transformer, string xslPath)
        : base(drive, outputFolder)
    {
        _transformer = transformer;
        _xslPath = xslPath;
    }

    public override string Extension => ".html";

    protected override string MimeType => "text/html";

    protected override IFragmentWriter CreateWriter() => new HtmlFragmentWriter(_transformer, _xslPath);
}
