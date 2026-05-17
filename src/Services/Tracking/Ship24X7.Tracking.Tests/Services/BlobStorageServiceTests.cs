using FluentAssertions;
using Ship24X7.Tracking.Infrastructure.Services;
using Xunit;

namespace Ship24X7.Tracking.Tests.Services;

/// <summary>
/// BlobStorageTests service implementation. Provides blobstoragetests functionality for the application.
/// </summary>
public class BlobStorageServiceTests : IDisposable
{
    private readonly string _testStoragePath;
    private readonly BlobStorageService _sut;

    public BlobStorageServiceTests()
    {
        _testStoragePath = Path.Combine(Path.GetTempPath(), $"TrackingTests_{Guid.NewGuid()}");
        _sut = new BlobStorageService(_testStoragePath);
    }

    [Fact]
    public async Task UploadDocumentAsync_WithValidFile_StoresFileAndReturnsUrl()
    {
        // Arrange
        var fileName = "test-document.pdf";
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var contentType = "application/pdf";

        // Act
        var fileUrl = await _sut.UploadDocumentAsync(fileName, content, contentType);

        // Assert
        fileUrl.Should().NotBeNullOrEmpty();
        fileUrl.Should().Contain(fileName);
        
        var filePath = Path.Combine(_testStoragePath, fileUrl);
        File.Exists(filePath).Should().BeTrue();
        
        var storedContent = await File.ReadAllBytesAsync(filePath);
        storedContent.Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task UploadDocumentAsync_WithSameFileName_GeneratesUniqueUrls()
    {
        // Arrange
        var fileName = "duplicate.pdf";
        var content1 = new byte[] { 1, 2, 3 };
        var content2 = new byte[] { 4, 5, 6 };

        // Act
        var fileUrl1 = await _sut.UploadDocumentAsync(fileName, content1, "application/pdf");
        var fileUrl2 = await _sut.UploadDocumentAsync(fileName, content2, "application/pdf");

        // Assert
        fileUrl1.Should().NotBe(fileUrl2);
        fileUrl1.Should().Contain(fileName);
        fileUrl2.Should().Contain(fileName);
    }

    [Fact]
    public async Task GenerateDownloadUrlAsync_WithDefaultExpiry_Returns60MinuteUrl()
    {
        // Arrange
        var fileUrl = "test-file.pdf";
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var downloadUrl = await _sut.GenerateDownloadUrlAsync(fileUrl);

        // Assert
        var afterGeneration = DateTime.UtcNow;
        
        downloadUrl.Should().NotBeNullOrEmpty();
        downloadUrl.Should().Contain(fileUrl);
        downloadUrl.Should().Contain("expires=");
        
        // Extract expiry timestamp from URL
        var expiresParam = downloadUrl.Split("expires=")[1];
        var expiryTime = DateTime.Parse(expiresParam, null, System.Globalization.DateTimeStyles.RoundtripKind);
        
        // Verify expiry is approximately 60 minutes from now (with some tolerance for test execution time)
        var expectedMinTime = beforeGeneration.AddMinutes(59);
        var expectedMaxTime = afterGeneration.AddMinutes(61);
        expiryTime.Should().BeOnOrAfter(expectedMinTime);
        expiryTime.Should().BeOnOrBefore(expectedMaxTime);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(120)]
    public async Task GenerateDownloadUrlAsync_WithCustomExpiry_ReturnsUrlWithSpecifiedExpiry(int expiryMinutes)
    {
        // Arrange
        var fileUrl = "test-file.pdf";
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var downloadUrl = await _sut.GenerateDownloadUrlAsync(fileUrl, expiryMinutes);

        // Assert
        var afterGeneration = DateTime.UtcNow;
        
        var expiresParam = downloadUrl.Split("expires=")[1];
        var expiryTime = DateTime.Parse(expiresParam, null, System.Globalization.DateTimeStyles.RoundtripKind);
        
        var expectedMinTime = beforeGeneration.AddMinutes(expiryMinutes - 1);
        var expectedMaxTime = afterGeneration.AddMinutes(expiryMinutes + 1);
        expiryTime.Should().BeOnOrAfter(expectedMinTime);
        expiryTime.Should().BeOnOrBefore(expectedMaxTime);
    }

    [Fact]
    public async Task GenerateDownloadUrlAsync_CalledMultipleTimes_GeneratesDifferentUrls()
    {
        // Arrange
        var fileUrl = "test-file.pdf";

        // Act
        var url1 = await _sut.GenerateDownloadUrlAsync(fileUrl, 60);
        await Task.Delay(100); // Small delay to ensure different timestamps
        var url2 = await _sut.GenerateDownloadUrlAsync(fileUrl, 60);

        // Assert
        url1.Should().NotBe(url2);
        url1.Should().Contain(fileUrl);
        url2.Should().Contain(fileUrl);
    }

    [Fact]
    public async Task DeleteDocumentAsync_WithExistingFile_DeletesFileAndReturnsTrue()
    {
        // Arrange
        var fileName = "to-delete.pdf";
        var content = new byte[] { 1, 2, 3 };
        var fileUrl = await _sut.UploadDocumentAsync(fileName, content, "application/pdf");

        // Act
        var result = await _sut.DeleteDocumentAsync(fileUrl);

        // Assert
        result.Should().BeTrue();
        var filePath = Path.Combine(_testStoragePath, fileUrl);
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteDocumentAsync_WithNonExistentFile_ReturnsFalse()
    {
        // Arrange
        var fileUrl = "non-existent-file.pdf";

        // Act
        var result = await _sut.DeleteDocumentAsync(fileUrl);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetDocumentAsync_WithExistingFile_ReturnsFileContent()
    {
        // Arrange
        var fileName = "retrieve-test.pdf";
        var content = new byte[] { 10, 20, 30, 40, 50 };
        var fileUrl = await _sut.UploadDocumentAsync(fileName, content, "application/pdf");

        // Act
        var retrievedContent = await _sut.GetDocumentAsync(fileUrl);

        // Assert
        retrievedContent.Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task GetDocumentAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var fileUrl = "non-existent-file.pdf";

        // Act
        var act = async () => await _sut.GetDocumentAsync(fileUrl);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage($"Document not found: {fileUrl}");
    }

    [Fact]
    public async Task UploadDocumentAsync_WithLargeFile_StoresSuccessfully()
    {
        // Arrange
        var fileName = "large-file.pdf";
        var content = new byte[5 * 1024 * 1024]; // 5 MB
        new Random().NextBytes(content);

        // Act
        var fileUrl = await _sut.UploadDocumentAsync(fileName, content, "application/pdf");

        // Assert
        fileUrl.Should().NotBeNullOrEmpty();
        var retrievedContent = await _sut.GetDocumentAsync(fileUrl);
        retrievedContent.Should().BeEquivalentTo(content);
    }

    [Fact]
    public void Constructor_CreatesStorageDirectoryIfNotExists()
    {
        // Arrange
        var newPath = Path.Combine(Path.GetTempPath(), $"NewStorage_{Guid.NewGuid()}");
        Directory.Exists(newPath).Should().BeFalse();

        // Act
        var service = new BlobStorageService(newPath);

        // Assert
        Directory.Exists(newPath).Should().BeTrue();

        // Cleanup
        Directory.Delete(newPath, true);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testStoragePath))
        {
            Directory.Delete(_testStoragePath, true);
        }
    }
}
