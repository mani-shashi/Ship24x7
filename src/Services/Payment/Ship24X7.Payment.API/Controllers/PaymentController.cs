using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Payment.Application.Commands;
using Ship24X7.Payment.Application.Queries;

namespace Ship24X7.Payment.API.Controllers;

/// <summary>
/// API controller for managing payment operations in the Ship24X7 payment service.
/// Handles HTTP requests for creating payment orders, verifying payments, and retrieving payment information.
/// This controller integrates with Razorpay payment gateway for processing online payments.
/// All endpoints require authentication via JWT token.
/// Workflow: Client creates payment order -> Razorpay processes payment -> Client verifies payment signature -> Payment captured.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentController> _logger;

    /// <summary>
    /// Initializes a new instance of the PaymentController class.
    /// Sets up dependencies for command/query processing and logging.
    /// </summary>
    /// <param name="mediator">MediatR instance for sending commands and queries to their respective handlers</param>
    /// <param name="logger">Logger instance for recording payment operations, errors, and diagnostic information</param>
    public PaymentController(IMediator mediator, ILogger<PaymentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new payment order for a shipment in the Razorpay payment gateway.
    /// Endpoint: POST /api/v1/payment/orders
    /// Logic flow:
    /// 1. Receives payment order creation request with shipment details and amount
    /// 2. Sends command to handler via MediatR for processing
    /// 3. Handler checks idempotency to prevent duplicate orders
    /// 4. Handler creates order in Razorpay with shipment metadata
    /// 5. Returns payment order details including Razorpay order ID and key for frontend integration
    /// 6. Handles service unavailability errors (503) and general errors (500)
    /// Used by frontend after shipment creation to initiate payment flow.
    /// </summary>
    /// <param name="command">Command containing shipment ID, tracking number, amount, currency, and idempotency key for order creation</param>
    /// <returns>
    /// 200 OK with PaymentOrderResponse containing payment order ID, Razorpay order ID, Razorpay key ID, amount, and currency.
    /// 503 Service Unavailable if Razorpay service is down or unreachable.
    /// 500 Internal Server Error for unexpected errors during order creation.
    /// </returns>
    [HttpPost("orders")]
    public async Task<IActionResult> CreatePaymentOrder([FromBody] CreatePaymentOrderCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error creating payment order");
            return StatusCode(503, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating payment order");
            return StatusCode(500, new { error = "An error occurred while creating payment order" });
        }
    }

    /// <summary>
    /// Verifies payment signature after successful payment on Razorpay frontend.
    /// Endpoint: POST /api/v1/payment/verify
    /// Logic flow:
    /// 1. Receives Razorpay order ID, payment ID, and signature from frontend after payment completion
    /// 2. Sends verification command to handler via MediatR
    /// 3. Handler validates HMAC-SHA256 signature using Razorpay key secret to ensure payment authenticity
    /// 4. If valid, updates payment order status to Captured and records payment ID and signature
    /// 5. Publishes PaymentCaptured domain event for downstream services (e.g., shipment service)
    /// 6. Returns success or error based on signature validation
    /// Critical security step: Prevents payment tampering and ensures only legitimate Razorpay payments are accepted.
    /// </summary>
    /// <param name="command">Command containing Razorpay order ID, payment ID, and signature for verification</param>
    /// <returns>
    /// 200 OK with success message if signature is valid and payment is verified.
    /// 400 Bad Request if signature is invalid or payment order not found.
    /// 500 Internal Server Error for unexpected errors during verification.
    /// </returns>
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentCommand command)
    {
        try
        {
            var isValid = await _mediator.Send(command);
            
            if (!isValid)
            {
                return BadRequest(new { error = "Invalid payment signature" });
            }

            return Ok(new { message = "Payment verified successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment");
            return StatusCode(500, new { error = "An error occurred while verifying payment" });
        }
    }

    /// <summary>
    /// Retrieves payment order details by payment order ID.
    /// Endpoint: GET /api/v1/payment/orders/{paymentOrderId}
    /// Logic flow:
    /// 1. Receives payment order ID from route parameter
    /// 2. Creates query with payment order ID
    /// 3. Sends query to handler via MediatR to fetch payment order from database
    /// 4. Returns payment order details if found, otherwise returns 404 Not Found
    /// Used by frontend and admin dashboards to check payment status and details.
    /// </summary>
    /// <param name="paymentOrderId">Unique identifier of the payment order to retrieve</param>
    /// <returns>
    /// 200 OK with payment order details including status, amount, Razorpay IDs, and timestamps.
    /// 404 Not Found if payment order with specified ID does not exist.
    /// 500 Internal Server Error for unexpected errors during retrieval.
    /// </returns>
    [HttpGet("orders/{paymentOrderId}")]
    public async Task<IActionResult> GetPaymentOrder(Guid paymentOrderId)
    {
        try
        {
            var query = new GetPaymentOrderQuery { PaymentOrderId = paymentOrderId };
            var paymentOrder = await _mediator.Send(query);

            if (paymentOrder == null)
            {
                return NotFound(new { error = "Payment order not found" });
            }

            return Ok(paymentOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment order");
            return StatusCode(500, new { error = "An error occurred while retrieving payment order" });
        }
    }

    /// <summary>
    /// Retrieves payment order details by shipment ID.
    /// Endpoint: GET /api/v1/payment/shipments/{shipmentId}
    /// Logic flow:
    /// 1. Receives shipment ID from route parameter
    /// 2. Creates query with shipment ID
    /// 3. Sends query to handler via MediatR to fetch payment order associated with the shipment
    /// 4. Returns payment order details if found, otherwise returns 404 Not Found
    /// Used by shipment service and frontend to check payment status for a specific shipment.
    /// Useful for displaying payment information on shipment tracking pages.
    /// </summary>
    /// <param name="shipmentId">Unique identifier of the shipment to retrieve payment order for</param>
    /// <returns>
    /// 200 OK with payment order details including status, amount, Razorpay IDs, and timestamps.
    /// 404 Not Found if no payment order exists for the specified shipment.
    /// 500 Internal Server Error for unexpected errors during retrieval.
    /// </returns>
    [HttpGet("shipments/{shipmentId}")]
    public async Task<IActionResult> GetPaymentOrderByShipment(Guid shipmentId)
    {
        try
        {
            var query = new GetPaymentOrderByShipmentQuery { ShipmentId = shipmentId };
            var paymentOrder = await _mediator.Send(query);

            if (paymentOrder == null)
            {
                return NotFound(new { error = "Payment order not found for this shipment" });
            }

            return Ok(paymentOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment order by shipment");
            return StatusCode(500, new { error = "An error occurred while retrieving payment order" });
        }
    }
}
