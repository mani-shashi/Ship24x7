# Payment Service Unit Tests

This test project contains comprehensive unit tests for the Ship24X7 Payment Service, covering all critical payment processing functionality.

## Test Coverage

### 1. HMAC-SHA256 Signature Verification Tests
**File:** `Services/HmacSignatureValidatorTests.cs`  
**Validates:** Requirements 17.2

Tests for cryptographic signature validation used in payment verification and webhook authentication:

- ✅ Valid payment signature verification
- ✅ Invalid signature rejection
- ✅ Tampered order ID detection
- ✅ Tampered payment ID detection
- ✅ Case-insensitive signature comparison
- ✅ Empty parameter handling
- ✅ Valid webhook signature verification
- ✅ Invalid webhook signature rejection
- ✅ Tampered webhook payload detection
- ✅ Configuration validation (missing secrets)

**Total Tests:** 12

### 2. Payment Order Creation Tests
**File:** `Handlers/CreatePaymentOrderCommandHandlerTests.cs`  
**Validates:** Requirements 17.2, 17.11

Tests for payment order creation with Razorpay API integration and idempotency handling:

- ✅ New payment order creation with Razorpay
- ✅ Payment order storage in database
- ✅ Idempotency key handling (returns existing order)
- ✅ Existing pending payment for shipment (returns existing)
- ✅ Existing captured payment for shipment (returns existing)
- ✅ Failed payment retry (creates new order)
- ✅ Correct Razorpay API parameters
- ✅ Razorpay service failure handling
- ✅ Payment order status set to Pending

**Total Tests:** 11

### 3. Refund Initiation Tests
**File:** `Handlers/InitiateRefundCommandHandlerTests.cs`  
**Validates:** Requirements 17.2, 17.11

Tests for refund initiation logic with validation and error handling:

- ✅ Valid refund initiation for captured payment
- ✅ Refund record storage in database
- ✅ Non-existent payment order handling
- ✅ Pending payment order rejection
- ✅ Failed payment order rejection
- ✅ Zero amount validation
- ✅ Negative amount validation
- ✅ Amount exceeding payment validation
- ✅ Partial refund support
- ✅ Razorpay service failure handling
- ✅ Correct Razorpay payment ID usage
- ✅ Refund status set to Initiated

**Total Tests:** 11

## Test Patterns

All tests follow xUnit testing patterns consistent with other services in the platform:

- **Arrange-Act-Assert** pattern for clarity
- **FluentAssertions** for readable assertions
- **Moq** for mocking dependencies
- **Descriptive test names** that explain the scenario and expected outcome
- **Requirement traceability** via XML documentation comments

## Running Tests

```bash
# Run all Payment Service tests
dotnet test src/Services/Payment/Ship24X7.Payment.Tests/Ship24X7.Payment.Tests.csproj

# Run with detailed output
dotnet test src/Services/Payment/Ship24X7.Payment.Tests/Ship24X7.Payment.Tests.csproj --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~HmacSignatureValidatorTests"
```

## Test Results

**Total Tests:** 34  
**Passed:** 34  
**Failed:** 0  
**Skipped:** 0

All tests pass successfully, validating the core payment processing functionality including:
- HMAC-SHA256 signature verification for payment security
- Webhook signature validation for Razorpay events
- Idempotency key handling to prevent duplicate payments
- Payment order creation with Razorpay API integration
- Refund initiation logic with comprehensive validation

## Dependencies

- xUnit 2.9.3
- FluentAssertions 8.9.0
- Moq 4.20.72
- Microsoft.NET.Test.Sdk 17.14.1
- Microsoft.EntityFrameworkCore.InMemory 8.0.0

## Notes

- Tests use in-memory configuration for Razorpay credentials
- All external dependencies (Razorpay API, repositories) are mocked
- Tests validate both success and failure scenarios
- Error messages are validated for proper exception handling
- Tests ensure idempotent behavior for payment operations
