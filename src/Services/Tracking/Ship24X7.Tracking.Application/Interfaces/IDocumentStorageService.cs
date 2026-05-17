namespace Ship24X7.Tracking.Application.Interfaces;

/// <summary>
/// IDocumentStorage service implementation. Provides idocumentstorage functionality for the application.
/// </summary>
public interface IDocumentStorageService
{
    Task<string> UploadDocumentAsync(string fileName, byte[] content, string contentType, CancellationToken cancellationToken = default);
    Task<string> GenerateDownloadUrlAsync(string fileUrl, int expiryMinutes = 60, CancellationToken cancellationToken = default);
    Task<bool> DeleteDocumentAsync(string fileUrl, CancellationToken cancellationToken = default);
}
