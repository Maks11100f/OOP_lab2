using OOP_lab2.Services;

namespace OOP_lab2.Saving;

public class XmlFragmentSaver : FragmentSaver
{
    public XmlFragmentSaver(GoogleDriveService drive, string outputFolder)
        : base(drive, outputFolder) { }

    public override string Extension => ".xml";

    protected override string MimeType => "text/xml";

    protected override IFragmentWriter CreateWriter() => new XmlFragmentWriter();
}
