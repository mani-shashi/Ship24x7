using Ship24X7.Tracking.Application.Interfaces;

namespace Ship24X7.Tracking.Infrastructure.Services;

/// <summary>
/// Blob storage service implementation for document storage and retrieval.
/// Provides file upload, download URL generation, deletion, and retrieval functionality.
/// Currently uses local file system storage; can be replaced with Azure Blob Storage in production.
/// </summary>
public class BlobStorageService : IDocumentStorageService
{
    private readonly string _storageBasePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobStorageService"/> class.
    /// Creates storage directory if it doesn't exist.
    /// </summary>
    /// <param name="storageBasePath">Base path for file storage on local file system.</param>
    public BlobStorageService(string storageBasePath)
    {
        _storageBasePath = storageBasePath;
        
        // Ensure storage directory exists
        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
        }
    }

    /// <summary>
    /// Uploads a document to blob storage.
    /// Process flow:
    /// 1. Generates unique file name using GUID prefix to prevent collisions
    /// 2. Combines base path with unique file name
    /// 3. Writes file content to disk asynchronously
    /// 4. Returns unique file name as file URL for database storage
    /// In production with Azure Blob Storage, this would upload to cloud storage and return blob URL.
    /// </summary>
    /// <param name="fileName">Original file name from upload.</param>
    /// <param name="content">File content as byte array.</param>
    /// <param name="contentType">MIME type of the file (e.g., "application/pdf", "image/jpeg").</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Unique file URL/identifier for accessing the uploaded document.</returns>
    public async Task<string> UploadDocumentAsync(string fileName, byte[] content, string contentType, CancellationToken cancellationToken = default)
    {
        // Generate unique file name to avoid collisions
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_storageBasePath, uniqueFileName);

        await File.WriteAllBytesAsync(filePath, content, cancellationToken);

        // Return relative path as file URL
        return uniqueFileName;
    }

    /// <summary>
    /// Generates a time-limited download URL for a document.
    /// Process flow:
    /// 1. Constructs download URL with file identifier
    /// 2. Adds expiry timestamp as query parameter
    /// 3. Returns URL for client download access
    /// In production with Azure Blob Storage, this would generate a SAS (Shared Access Signature) token.
    /// </summary>
    /// <param name="fileUrl">File URL/identifier returned from UploadDocumentAsync.</param>
    /// <param name="expiryMinutes">Number of minutes until download URL expires (default 60).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Time-limited download URL with expiry timestamp.</returns>
    public Task<string> GenerateDownloadUrlAsync(string fileUrl, int expiryMinutes = 60, CancellationToken cancellationToken = default)
    {
        // For local file storage, return the file URL with a timestamp
        // In production with Azure Blob, this would generate a SAS token
        var downloadUrl = $"/api/v1/documents/download/{fileUrl}?expires={DateTime.UtcNow.AddMinutes(expiryMinutes):O}";
        return Task.FromResult(downloadUrl);
    }

    /// <summary>
    /// Deletes a document from blob storage.
    /// Removes file from file system if it exists.
    /// </summary>
    /// <param name="fileUrl">File URL/identifier to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if file was deleted successfully; false if file not found.</returns>
    public Task<bool> DeleteDocumentAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_storageBasePath, fileUrl);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Retrieves document content from blob storage.
    /// Reads file from file system and returns as byte array.
    /// </summary>
    /// <param name="fileUrl">File URL/identifier to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Document content as byte array.</returns>
    /// <exception cref="FileNotFoundException">Thrown when document file does not exist.</exception>
    public async Task<byte[]> GetDocumentAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_storageBasePath, fileUrl);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Document not found: {fileUrl}");
        }

        return await File.ReadAllBytesAsync(filePath, cancellationToken);
    }
}
