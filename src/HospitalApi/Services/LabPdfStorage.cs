namespace HospitalApi.Services;

public interface ILabPdfStorage
{
    Task SaveAsync(int reportId, byte[] content, CancellationToken cancellationToken = default);
    Task<byte[]?> TryReadAsync(int reportId, CancellationToken cancellationToken = default);
    void Delete(int reportId);
    bool Exists(int reportId);
}

public sealed class LabPdfStorage : ILabPdfStorage
{
    private readonly string _root;

    public LabPdfStorage(IWebHostEnvironment env)
    {
        _root = Path.Combine(env.ContentRootPath, "Data", "LabPdfs");
        Directory.CreateDirectory(_root);
    }

    private string FilePath(int reportId) => Path.Combine(_root, $"{reportId}.pdf");

    public Task SaveAsync(int reportId, byte[] content, CancellationToken cancellationToken = default) =>
        File.WriteAllBytesAsync(FilePath(reportId), content, cancellationToken);

    public async Task<byte[]?> TryReadAsync(int reportId, CancellationToken cancellationToken = default)
    {
        var path = FilePath(reportId);
        return File.Exists(path) ? await File.ReadAllBytesAsync(path, cancellationToken) : null;
    }

    public void Delete(int reportId)
    {
        var path = FilePath(reportId);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public bool Exists(int reportId) => File.Exists(FilePath(reportId));
}
