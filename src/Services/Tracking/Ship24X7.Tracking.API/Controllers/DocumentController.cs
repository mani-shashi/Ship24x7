using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Tracking.Application.Commands;
using Ship24X7.Tracking.Application.Queries;

namespace Ship24X7.Tracking.API.Controllers;

/// <summary>
/// API controller for managing shipment document operations in the Ship24X7 tracking service.
/// Handles HTTP requests for uploading, listing, and downloading shipment-related documents.
/// Supports multiple document types: ShippingLabel, CustomsDeclaration, Invoice, PackingList, and Other.
/// Documents stored in Azure Blob Storage with secure access via time-limited SAS URLs.
/// File size limit: 10 MB. Supported formats: PDF, JPG, PNG.
/// Workflow: Document uploaded -> Stored in blob storage -> Metadata saved to database -> Download URL generated on demand.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DocumentController> _logger;

    /// <summary>
    /// Initializes a new instance of the DocumentController class.
    /// Sets up dependencies for command/query processing and logging.
    /// </summary>
    /// <param name="mediator">MediatR instance for sending commands and queries to their respective handlers</param>
    /// <param name="logger">Logger instance for recording document operations, errors, and diagnostic information</param>
    public DocumentController(IMediator mediator, ILogger<DocumentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Uploads a document for a shipment.
    /// Endpoint: POST /api/v1/document/upload
    /// Logic flow:
    /// 1. Receives multipart form data with file, shipment ID, tracking number, document type, and uploader ID
    /// 2. Validates file size does not exceed 10 MB limit
    /// 3. If file too large, returns 422 Unprocessable Entity with error message
    /// 4. Validates file content type is PDF, JPG, or PNG
    /// 5. If file type invalid, returns 422 Unprocessable Entity with error message
    /// 6. Reads file content into memory stream and converts to byte array
    /// 7. Creates UploadDocumentCommand with file data and metadata
    /// 8. Sends command to handler via MediatR for processing
    /// 9. Handler uploads file to Azure Blob Storage with unique filename
    /// 10. Handler creates Document entity with file URL, metadata, and saves to database
    /// 11. Handler publishes DocumentUploaded domain event for downstream services
    /// 12. Returns document details with 201 Created status
    /// 13. Location header points to GetDocumentList endpoint for the shipment
    /// Used by admin users, hub operators, and automated systems to attach documents to shipments.
    /// </summary>
    /// <param name="file">File to upload (multipart form data)</param>
    /// <param name="shipmentId">Unique identifier of the shipment to attach document to</param>
    /// <param name="trackingNumber">Tracking number of the shipment</param>
    /// <param name="documentType">Type of document (ShippingLabel, CustomsDeclaration, Invoice, PackingList, Other)</param>
    /// <param name="uploadedBy">Unique identifier of the user uploading the document</param>
    /// <returns>
    /// 201 Created with DocumentResponse containing document ID, shipment ID, document type, filename, file URL, content type, file size, and upload timestamp.
    /// 422 Unprocessable Entity if file size exceeds 10 MB, file type is invalid, or upload fails.
    /// 500 Internal Server Error for unexpected errors during upload.
    /// </returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument([FromForm] IFormFile file, [FromForm] Guid shipmentId, 
        [FromForm] string trackingNumber, [FromForm] string documentType, [FromForm] Guid uploadedBy)
    {
        try
        {
            // Validate file size (max 10 MB)
            const long maxFileSizeBytes = 10 * 1024 * 1024;
            if (file.Length > maxFileSizeBytes)
            {
                return UnprocessableEntity(new { error = "File size exceeds 10 MB limit" });
            }

            // Validate file type
            var allowedContentTypes = new[] { "application/pdf", "image/jpeg", "image/jpg", "image/png" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
            {
                return UnprocessableEntity(new { error = "File type must be PDF, JPG, or PNG" });
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var command = new UploadDocumentCommand
            {
                ShipmentId = shipmentId,
                TrackingNumber = trackingNumber,
                DocumentType = Enum.Parse<Domain.Enums.DocumentType>(documentType),
                FileName = file.FileName,
                FileContent = memoryStream.ToArray(),
                ContentType = file.ContentType,
                UploadedBy = uploadedBy
            };

            var response = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetDocumentList), new { shipmentId = response.ShipmentId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return StatusCode(500, new { error = "An error occurred while uploading the document" });
        }
    }

    /// <summary>
    /// Retrieves list of all documents for a shipment by shipment ID.
    /// Endpoint: GET /api/v1/document/shipment/{shipmentId}
    /// Logic flow:
    /// 1. Receives shipment ID from route parameter and optional customer ID from query string
    /// 2. Creates query with shipment ID and customer ID
    /// 3. Sends query to handler via MediatR to fetch all documents for the shipment
    /// 4. If customer ID provided, validates customer owns the shipment (authorization check)
    /// 5. If customer does not own shipment, returns 403 Forbidden
    /// 6. Handler retrieves documents from database ordered by upload timestamp (newest first)
    /// 7. Returns list of documents with metadata (ID, type, filename, file size, upload timestamp)
    /// Used by customers, customer service, and admin dashboards to view all shipment documents.
    /// </summary>
    /// <param name="shipmentId">Unique identifier of the shipment to retrieve documents for</param>
    /// <param name="customerId">Optional customer ID for authorization check (ensures customer owns the shipment)</param>
    /// <returns>
    /// 200 OK with list of DocumentResponse objects containing document metadata.
    /// 403 Forbidden if customer ID provided but customer does not own the shipment.
    /// 500 Internal Server Error for unexpected errors during retrieval.
    /// </returns>
    [HttpGet("shipment/{shipmentId}")]
    public async Task<IActionResult> GetDocumentList(Guid shipmentId, [FromQuery] Guid? customerId = null)
    {
        try
        {
            var query = new GetDocumentListQuery { ShipmentId = shipmentId, CustomerId = customerId };
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document list");
            return StatusCode(500, new { error = "An error occurred while retrieving documents" });
        }
    }

    /// <summary>
    /// Generates a time-limited download URL for a document.
    /// Endpoint: GET /api/v1/document/{documentId}/download-url
    /// Logic flow:
    /// 1. Receives document ID from route parameter and optional customer ID from query string
    /// 2. Creates query with document ID and customer ID
    /// 3. Sends query to handler via MediatR to generate download URL
    /// 4. Handler retrieves document from database to get file URL and shipment ID
    /// 5. If document not found, returns 404 Not Found
    /// 6. If customer ID provided, validates customer owns the shipment (authorization check)
    /// 7. If customer does not own shipment, returns 403 Forbidden
    /// 8. Handler generates SAS (Shared Access Signature) URL for blob storage with 1-hour expiration
    /// 9. Returns download URL for direct file access
    /// Used by frontend to download documents securely without exposing permanent storage URLs.
    /// SAS URL expires after 1 hour for security.
    /// </summary>
    /// <param name="documentId">Unique identifier of the document to generate download URL for</param>
    /// <param name="customerId">Optional customer ID for authorization check (ensures customer owns the shipment)</param>
    /// <returns>
    /// 200 OK with downloadUrl property containing time-limited SAS URL for file download.
    /// 403 Forbidden if customer ID provided but customer does not own the shipment.
    /// 404 Not Found if document does not exist.
    /// 500 Internal Server Error for unexpected errors during URL generation.
    /// </returns>
    [HttpGet("{documentId}/download-url")]
    public async Task<IActionResult> GetDocumentDownloadUrl(Guid documentId, [FromQuery] Guid? customerId = null)
    {
        try
        {
            var query = new GetDocumentDownloadUrlQuery { DocumentId = documentId, CustomerId = customerId };
            var downloadUrl = await _mediator.Send(query);
            return Ok(new { downloadUrl });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download URL");
            return StatusCode(500, new { error = "An error occurred while generating download URL" });
        }
    }
}
