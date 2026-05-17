# Ship24X7 API Gateway

The API Gateway is the single entry point for all client requests to the Ship24X7 platform. It uses Ocelot to route requests to the appropriate microservices and provides cross-cutting concerns like authentication, rate limiting, circuit breaking, and CORS.

## Features

- **JWT Authentication**: Validates JWT tokens before forwarding to downstream services
- **Rate Limiting**: 
  - 100 requests/minute for authenticated users
  - 20 requests/minute for public endpoints (per IP)
- **Circuit Breaker**: 5 failures trigger 30-second break
- **Retry Policy**: Configured via QoSOptions with exponential backoff
- **CORS**: Configurable allowed origins from environment variables
- **CorrelationId Propagation**: Adds X-Correlation-Id header to all requests
- **Health Checks**: `/health` endpoint for service status
- **Prometheus Metrics**: `/metrics` endpoint for monitoring
- **HTTPS Redirection**: Enforces HTTPS-only communication
- **Graceful Shutdown**: 30-second timeout for in-flight requests

## Ports

- **HTTP**: 8000
- **HTTPS**: 9000

## Environment Variables

Required environment variables:

```bash
# JWT Configuration
JWT_SECRET_KEY=your-super-secret-jwt-signing-key-min-32-chars-long
JWT_ISSUER=Ship24X7
JWT_AUDIENCE=Ship24X7API

# CORS Configuration
CORS_ALLOWED_ORIGINS=http://localhost:4200,https://localhost:4200
```

## Routes

### Auth Service (Port 9001)
- `/api/v1/auth/**` - Public endpoints (rate limit: 20 req/min per IP)

### Shipment Service (Port 9002)
- `/api/v1/shipments/**` - Authenticated endpoints (rate limit: 100 req/min per user)

### Tracking Service (Port 9003)
- `/api/v1/tracking/public/**` - Public tracking (rate limit: 20 req/min per IP)
- `/api/v1/tracking/**` - Authenticated endpoints (rate limit: 100 req/min per user)

### Notification Service (Port 9004)
- `/api/v1/notifications/**` - Authenticated endpoints (rate limit: 100 req/min per user)

### Payment Service (Port 9005)
- `/api/v1/payments/webhook/**` - Razorpay webhooks (rate limit: 100 req/min)
- `/api/v1/payments/**` - Authenticated endpoints (rate limit: 100 req/min per user)

## Circuit Breaker Configuration

Each route is configured with:
- **Exceptions Allowed Before Breaking**: 5
- **Duration of Break**: 30 seconds
- **Timeout**: 10 seconds

## Running the Gateway

```bash
# Set environment variables
export JWT_SECRET_KEY="your-secret-key-here"
export JWT_ISSUER="Ship24X7"
export JWT_AUDIENCE="Ship24X7API"
export CORS_ALLOWED_ORIGINS="http://localhost:4200"

# Run the gateway
dotnet run
```

## Docker

```bash
docker build -t ship24x7-gateway .
docker run -p 8000:8000 -p 9000:9000 \
  -e JWT_SECRET_KEY="your-secret-key" \
  -e CORS_ALLOWED_ORIGINS="http://localhost:4200" \
  ship24x7-gateway
```

## Monitoring

- **Health Check**: `GET http://localhost:8000/health`
- **Metrics**: `GET http://localhost:8000/metrics`

## Logging

Logs are written to:
- Console (structured JSON)
- File: `logs/gateway-{Date}.log` (daily rollover)

All logs include:
- Timestamp
- Log level
- CorrelationId
- Message
- Exception (if any)

## Security

- All HTTP requests are redirected to HTTPS
- JWT tokens are validated using HS256 algorithm
- CORS is enforced based on configured origins
- Rate limiting prevents abuse
- Circuit breaker prevents cascading failures
