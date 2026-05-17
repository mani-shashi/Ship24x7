# Inter-Service Event Flow Testing Guide

This document describes how to test the inter-service event flows for the Ship24X7 platform.

## Prerequisites

1. RabbitMQ must be running (default: amqp://guest:guest@localhost:5672)
2. All microservices must be running:
   - Shipment Service (port 9002)
   - Payment Service (port 9005)
   - Tracking Service (port 9003)
   - Notification Service (port 9004)
3. SQL Server must be running with all databases created

## Event Flow 1: ShipmentBooked

**Publisher:** Shipment Service  
**Consumers:** Notification Service  
**Routing Key:** `shipment.booked`

### Test Steps:
1. Create a draft shipment via POST `/api/v1/shipment`
2. Confirm the shipment via POST `/api/v1/shipment/{id}/confirm`
3. Verify:
   - Shipment status changes to `Booked`
   - ShipmentBooked event is published to RabbitMQ
   - Notification Service receives the event (check logs)
   - Booking confirmation email is sent (check NotificationLog table)

### Expected Logs:
```
[Shipment Service] Publishing ShipmentBooked event for tracking number SHIP24X7-...
[Notification Service] Received event ShipmentBooked with CorrelationId ...
[Notification Service] Processing ShipmentBooked event for SHIP24X7-...
[Notification Service] Booking confirmation email sent for SHIP24X7-...
```

## Event Flow 2: PaymentCaptured

**Publisher:** Payment Service  
**Consumers:** Shipment Service, Notification Service  
**Routing Key:** `payment.captured`

### Test Steps:
1. Create a payment order via POST `/api/v1/payment/create`
2. Verify payment via POST `/api/v1/payment/verify` (with valid Razorpay signature)
3. Verify:
   - PaymentOrder status changes to `Captured`
   - PaymentCaptured event is published to RabbitMQ
   - Shipment Service receives the event and updates shipment status to `Paid`
   - Notification Service receives the event and sends payment confirmation email

### Expected Logs:
```
[Payment Service] Publishing PaymentCaptured event for shipment ...
[Shipment Service] Received event PaymentCaptured with CorrelationId ...
[Shipment Service] Processing PaymentCaptured event for Shipment ...
[Shipment Service] Shipment ... status updated to Paid
[Notification Service] Received event PaymentCaptured with CorrelationId ...
[Notification Service] Payment confirmation email sent for ...
```

## Event Flow 3: ShipmentDelivered

**Publisher:** Tracking Service  
**Consumers:** Notification Service  
**Routing Key:** `shipment.delivered`

### Test Steps:
1. Capture delivery proof via POST `/api/v1/tracking/delivery-proof`
2. Verify:
   - DeliveryProof record is created
   - ShipmentDelivered event is published to RabbitMQ
   - Notification Service receives the event
   - Delivery confirmation email/SMS is sent

### Expected Logs:
```
[Tracking Service] Publishing ShipmentDelivered event for tracking number SHIP24X7-...
[Notification Service] Received event ShipmentDelivered with CorrelationId ...
[Notification Service] Processing ShipmentDelivered event for SHIP24X7-...
[Notification Service] Delivery confirmation email sent for SHIP24X7-...
```

## Event Flow 4: ShipmentDelayed

**Publisher:** Tracking Service  
**Consumers:** Notification Service  
**Routing Key:** `shipment.delayed`

### Test Steps:
1. Record a tracking event with `IsException = true` via POST `/api/v1/tracking/events`
2. Ensure shipment is in InTransit, PickedUp, or OutForDelivery status
3. Verify:
   - TrackingEvent is recorded with IsException flag
   - ShipmentDelayed event is published to RabbitMQ
   - Notification Service receives the event
   - Delay notification email is sent

### Expected Logs:
```
[Tracking Service] Publishing ShipmentDelayed event for tracking number SHIP24X7-...
[Notification Service] Received event ShipmentDelayed with CorrelationId ...
[Notification Service] Processing ShipmentDelayed event for SHIP24X7-...
[Notification Service] Delay notification email sent for SHIP24X7-...
```

## Event Flow 5: PaymentFailed

**Publisher:** Payment Service  
**Consumers:** Shipment Service  
**Routing Key:** `payment.failed`

### Test Steps:
1. Trigger a payment failure (via Razorpay webhook or failed payment verification)
2. Verify:
   - PaymentOrder status changes to `Failed`
   - PaymentFailed event is published to RabbitMQ
   - Shipment Service receives the event
   - Shipment status changes to `PaymentFailed`

### Expected Logs:
```
[Payment Service] Publishing PaymentFailed event for shipment ...
[Shipment Service] Received event PaymentFailed with CorrelationId ...
[Shipment Service] Processing PaymentFailed event for Shipment ...
[Shipment Service] Shipment ... status updated to PaymentFailed
```

## Event Flow 6: RefundProcessed

**Publisher:** Payment Service  
**Consumers:** None (currently)  
**Routing Key:** `refund.processed`

### Test Steps:
1. Initiate a refund via POST `/api/v1/payment/refund`
2. Process refund via Razorpay webhook
3. Verify:
   - PaymentRefund status changes to `Processed`
   - RefundProcessed event is published to RabbitMQ

### Expected Logs:
```
[Payment Service] Publishing RefundProcessed event for refund ...
```

## Correlation ID Tracing

All events include a CorrelationId in the message headers. This allows tracing a request across all services.

### Example Trace:
1. User creates shipment → CorrelationId: `abc123`
2. ShipmentBooked event published with CorrelationId: `abc123`
3. Notification Service processes event with CorrelationId: `abc123`
4. All logs include CorrelationId: `abc123`

### Query Logs by CorrelationId:
```bash
# Using grep on log files
grep "abc123" logs/ShipmentService-*.log
grep "abc123" logs/NotificationService-*.log
grep "abc123" logs/PaymentService-*.log
```

## Dead Letter Queue Testing

To test DLQ routing:

1. Stop the Notification Service
2. Publish an event (e.g., ShipmentBooked)
3. Start the Notification Service
4. The consumer will fail to process the event
5. After 3 retries with exponential backoff, the message should be moved to DLQ

### Expected Behavior:
- Retry 1: Wait 2^0 = 1 second
- Retry 2: Wait 2^1 = 2 seconds
- Retry 3: Wait 2^2 = 4 seconds
- After 3 retries: Message moved to DLQ

### Check DLQ:
```bash
# Using RabbitMQ Management UI
http://localhost:15672
# Navigate to Queues → notification.shipment.events.dlq
```

## Troubleshooting

### Event Not Received
1. Check RabbitMQ is running: `docker ps | grep rabbitmq`
2. Check exchange exists: RabbitMQ Management UI → Exchanges → `ship24x7.events`
3. Check queue bindings: RabbitMQ Management UI → Queues → Check bindings
4. Check consumer logs for connection errors

### Event Received But Not Processed
1. Check consumer logs for exceptions
2. Verify MediatR handlers are registered
3. Check database connectivity
4. Verify CorrelationId is propagated

### Multiple Event Deliveries
1. Ensure consumers are using `autoAck: false`
2. Verify `BasicAck` is called after successful processing
3. Check for duplicate consumer registrations
