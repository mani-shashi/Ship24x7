# Ship24X7 — Low-Level Design (LLD)

**Version:** 1.0  
**Date:** May 2026  
**Status:** Final

---

## 1. Introduction

This document describes the internal design of each microservice, data models, API contracts, class relationships, and key algorithms in the Ship24X7 platform.

---

## 2. Shared Infrastructure

### 2.1 BaseEntity

All domain entities inherit from `BaseEntity` in `Ship24X7.Shared`.

```mermaid
classDiagram
    class BaseEntity {
        +string CorrelationId
        +DateTime CreatedAt
        +Guid CreatedBy
        +DateTime? UpdatedAt
        +Guid? UpdatedBy
        +bool IsDeleted
        +DateTime? DeletedAt
        +Guid? DeletedBy
    }

    BaseEntity <|-- User
    BaseEntity <|-- Shipment
    BaseEntity <|-- TrackingEvent
    BaseEntity <|-- PaymentOrder
    BaseEntity <|-- PaymentRefund
```

### 2.2 CorrelationIdMiddleware

Registered in every service's ASP.NET pipeline. Extracts `X-Correlation-Id` from the incoming request header or generates a new GUID. Pushes it to:
1. Response headers (for client-side tracing)
2. Serilog `LogContext` (for log enrichment)
3. `HttpContext.Items["CorrelationId"]` (for service layer access)

### 2.3 Serilog Configuration

- Console sink for development
- Daily rolling file sink: 30-day retention, 100 MB per file
- Enriched with `CorrelationId`, `ServiceName`, `Environment`

---

## 3. Auth Service

### 3.1 Domain Model

```mermaid
classDiagram
    class User {
        +Guid Id
        +string Email
        +string PasswordHash
        +string FullName
        +string PhoneNumber
        +bool EmailVerified
        +bool PhoneVerified
        +bool IsActive
        +bool IsLocked
        +DateTime? LockoutEnd
        +int FailedLoginAttempts
        +DateTime? LastLoginAt
        +string? ProfilePhotoUrl
    }

    class RefreshToken {
        +Guid Id
        +Guid UserId
        +string Token
        +DateTime ExpiresAt
        +bool IsRevoked
        +DateTime? RevokedAt
    }

    class ExternalLogin {
        +Guid Id
        +Guid UserId
        +string Provider
        +string ExternalId
    }

    class MfaSettings {
        +Guid Id
        +Guid UserId
        +string TotpSecret
        +bool IsEnabled
        +string[] BackupCodes
    }

    class VerificationToken {
        +Guid Id
        +Guid UserId
        +string Token
        +string Type
        +DateTime ExpiresAt
        +bool IsUsed
    }

    class UserRole {
        +Guid Id
        +Guid UserId
        +string Role
    }

    class UserAddress {
        +Guid Id
        +Guid UserId
        +string Label
        +string ContactName
        +string ContactPhone
        +string AddressLine1
        +string? AddressLine2
        +string City
        +string State
        +string PostalCode
        +string Country
        +string Type
        +bool IsDefault
    }

    class UserPreferences {
        +Guid Id
        +Guid UserId
        +bool EmailNotifications
        +bool SmsNotifications
        +bool PushNotifications
        +bool MarketingEmails
        +string Theme
        +string Language
    }

    User "1" --> "*" RefreshToken
    User "1" --> "*" ExternalLogin
    User "1" --> "0..1" MfaSettings
    User "1" --> "*" VerificationToken
    User "1" --> "*" UserRole
    User "1" --> "*" UserAddress
    User "1" --> "0..1" UserPreferences
```

### 3.2 Authentication Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant GW as Gateway
    participant A as Auth Service
    participant DB as Auth DB

    C->>GW: POST /api/v1/auth/login {email, password}
    GW->>A: Forward request
    A->>DB: Find user by email
    DB-->>A: User record
    A->>A: Verify BCrypt hash
    alt MFA enabled
        A-->>C: {requiresMfa: true}
        C->>GW: POST /api/v1/auth/login {email, password, mfaCode}
        GW->>A: Forward
        A->>A: Validate TOTP code
    end
    A->>DB: Create RefreshToken (7-day TTL)
    A-->>C: {accessToken (15min), refreshToken, user}
```

### 3.3 Token Refresh Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant A as Auth Service
    participant DB as Auth DB

    C->>A: POST /api/v1/auth/refresh {refreshToken}
    A->>DB: Find RefreshToken
    alt Token valid and not revoked
        A->>DB: Revoke old token (rotation)
        A->>DB: Create new RefreshToken
        A-->>C: {newAccessToken, newRefreshToken}
    else Token invalid/expired
        A-->>C: 401 Unauthorized
    end
```

### 3.4 Google OAuth Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant A as Auth Service
    participant G as Google OAuth

    C->>A: GET /api/v1/auth/oauth/google
    A-->>C: Redirect to Google consent screen
    C->>G: User grants consent
    G->>A: GET /api/v1/auth/oauth/callback?code=...
    A->>G: Exchange code for tokens
    G-->>A: {id_token, access_token}
    A->>A: Decode id_token, extract email/name
    A->>A: Find or create User + ExternalLogin
    A-->>C: Redirect to frontend with JWT
```

### 3.5 API Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | /api/v1/auth/register | None | Register new user |
| POST | /api/v1/auth/login | None | Login with email/password |
| POST | /api/v1/auth/refresh | None | Refresh access token |
| POST | /api/v1/auth/logout | Bearer | Revoke refresh token |
| GET | /api/v1/auth/me | Bearer | Get current user |
| PUT | /api/v1/auth/me | Bearer | Update profile |
| PUT | /api/v1/auth/me/password | Bearer | Change password |
| GET | /api/v1/auth/verify-email | None | Verify email token |
| POST | /api/v1/auth/mfa/setup | Bearer | Setup TOTP MFA |
| POST | /api/v1/auth/mfa/verify | Bearer | Verify MFA code |
| POST | /api/v1/auth/mfa/disable | Bearer | Disable MFA |
| GET | /api/v1/auth/addresses | Bearer | Get address book |
| POST | /api/v1/auth/addresses | Bearer | Save address |
| PUT | /api/v1/auth/addresses/{id} | Bearer | Update address |
| DELETE | /api/v1/auth/addresses/{id} | Bearer | Delete address |
| GET | /api/v1/auth/preferences | Bearer | Get preferences |
| PUT | /api/v1/auth/preferences | Bearer | Update preferences |
| GET | /api/v1/auth/users | Admin | List all users |
| POST | /api/v1/auth/users/{id}/activate | Admin | Activate user |
| POST | /api/v1/auth/users/{id}/deactivate | Admin | Deactivate user |

---

## 4. Shipment Service

### 4.1 Domain Model

```mermaid
classDiagram
    class Shipment {
        +Guid Id
        +string TrackingNumber
        +Guid CustomerId
        +ShipmentStatus Status
        +Guid SenderAddressId
        +Guid ReceiverAddressId
        +Guid ServiceRateId
        +decimal ActualWeight
        +decimal VolumetricWeight
        +decimal ChargeableWeight
        +decimal BaseRate
        +decimal FuelSurcharge
        +decimal InsuranceCost
        +decimal TotalCost
        +string Currency
        +DateTime EstimatedDeliveryDate
        +DateTime? ActualDeliveryDate
        +Guid? OriginHubId
        +Guid? DestinationHubId
        +bool IsFragile
        +bool RequiresRefrigeration
        +decimal? DeclaredValue
        +string IdempotencyKey
    }

    class Address {
        +Guid Id
        +string ContactName
        +string ContactPhone
        +string AddressLine1
        +string? AddressLine2
        +string City
        +string State
        +string PostalCode
        +string Country
    }

    class ServiceRate {
        +Guid Id
        +string ServiceType
        +decimal BaseRatePerKg
        +decimal MinimumCharge
        +decimal FuelSurchargePercent
        +int EstimatedDeliveryDays
        +bool IsActive
    }

    class Hub {
        +Guid Id
        +string Name
        +string City
        +string State
        +string PostalCode
        +bool IsActive
        +int Capacity
    }

    class ShipmentItem {
        +Guid Id
        +Guid ShipmentId
        +string Description
        +int Quantity
        +decimal Weight
        +decimal Length
        +decimal Width
        +decimal Height
        +string PackageType
    }

    class Pickup {
        +Guid Id
        +Guid ShipmentId
        +DateTime PickupDate
        +string TimeSlot
        +string ConfirmationNumber
        +string? DriverId
        +bool IsCompleted
    }

    Shipment "1" --> "1" Address : SenderAddress
    Shipment "1" --> "1" Address : ReceiverAddress
    Shipment "1" --> "1" ServiceRate
    Shipment "1" --> "0..1" Hub : OriginHub
    Shipment "1" --> "0..1" Hub : DestinationHub
    Shipment "1" --> "*" ShipmentItem
    Shipment "1" --> "0..1" Pickup
```

### 4.2 Shipment Status State Machine

```mermaid
stateDiagram-v2
    [*] --> Draft : Create Shipment
    Draft --> Booked : Confirm Shipment
    Booked --> PaymentPending : Initiate Payment
    PaymentPending --> Paid : Payment Captured
    PaymentPending --> PaymentFailed : Payment Failed
    PaymentFailed --> PaymentPending : Retry Payment
    Paid --> PickedUp : Pickup Completed
    PickedUp --> InTransit : Depart Origin Hub
    InTransit --> InTransit : Hub Transfer
    InTransit --> Delayed : Exception Flagged
    Delayed --> InTransit : Exception Resolved
    InTransit --> OutForDelivery : Arrive Destination Hub
    OutForDelivery --> Delivered : Delivery Confirmed
    OutForDelivery --> Failed : Delivery Attempt Failed
    Failed --> Returned : Return Initiated
    Returned --> [*]
    Delivered --> [*]
    Booked --> Cancelled : Customer Cancels
    Cancelled --> [*]
```

### 4.3 Pricing Algorithm

```
VolumetricWeight = (Length × Width × Height) / 5000
ChargeableWeight = MAX(ActualWeight, VolumetricWeight)
BaseRate = MAX(ChargeableWeight × ServiceRate.BaseRatePerKg, ServiceRate.MinimumCharge)
FuelSurcharge = BaseRate × ServiceRate.FuelSurchargePercent / 100
InsuranceCost = DeclaredValue × 0.01  (if insurance selected)
TotalCost = BaseRate + FuelSurcharge + InsuranceCost
```

### 4.4 Tracking Number Format

```
SHIP24X7-YYYYMMDDNNNNNN
Example: SHIP24X7-20260504000001
```

### 4.5 API Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | /api/v1/shipment/shipments | Bearer | Create shipment |
| GET | /api/v1/shipment/shipments | Bearer | List shipments |
| GET | /api/v1/shipment/shipments/{id} | Bearer | Get shipment |
| GET | /api/v1/shipment/shipments/tracking/{num} | Bearer | Get by tracking number |
| POST | /api/v1/shipment/shipments/{id}/confirm | Bearer | Confirm shipment |
| POST | /api/v1/shipment/shipments/{id}/status | Admin | Update status |
| POST | /api/v1/shipment/shipments/{id}/pickup | Bearer | Schedule pickup |
| POST | /api/v1/shipment/shipments/{id}/pickup/complete | Hub | Complete pickup |
| POST | /api/v1/shipment/shipments/bulk-archive | Bearer | Bulk archive |
| GET | /api/v1/shipment/dashboard/summary | Bearer | Dashboard stats |
| GET | /api/v1/Rate | Bearer | Get service rates |
| GET | /api/v1/shipment/hubs | Admin | List hubs |
| GET | /api/v1/shipment/templates | Bearer | List templates |
| POST | /api/v1/shipment/templates | Bearer | Create template |

---

## 5. Tracking Service

### 5.1 Domain Model

```mermaid
classDiagram
    class TrackingEvent {
        +Guid Id
        +Guid ShipmentId
        +string TrackingNumber
        +ShipmentStatus Status
        +string Description
        +string Location
        +DateTime EventTimestamp
        +bool IsException
        +string? ExceptionReason
        +string? RecordedBy
    }

    class TrackingRecord {
        +string TrackingNumber
        +ShipmentStatus CurrentStatus
        +DateTime? EstimatedDelivery
        +List~TrackingEvent~ Events
    }

    TrackingRecord "1" --> "*" TrackingEvent
```

### 5.2 Event Consumption Flow

```mermaid
sequenceDiagram
    participant S as Shipment Service
    participant MQ as RabbitMQ
    participant T as Tracking Service
    participant DB as Tracking DB

    S->>MQ: Publish ShipmentStatusChanged {shipmentId, trackingNumber, newStatus, location}
    MQ->>T: Deliver message
    T->>DB: INSERT TrackingEvent {status, description, location, timestamp}
    T->>T: Update current status in TrackingRecord
```

### 5.3 API Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | /api/v1/tracking/{trackingNumber} | None | Get tracking info |
| POST | /api/v1/tracking/events | Internal | Record tracking event |

---

## 6. Payment Service

### 6.1 Domain Model

```mermaid
classDiagram
    class PaymentOrder {
        +Guid Id
        +Guid ShipmentId
        +string TrackingNumber
        +string RazorpayOrderId
        +string? RazorpayPaymentId
        +string? RazorpaySignature
        +decimal Amount
        +string Currency
        +PaymentStatus Status
        +DateTime? CapturedAt
        +string? FailureReason
        +string IdempotencyKey
    }

    class PaymentRefund {
        +Guid Id
        +Guid PaymentOrderId
        +decimal Amount
        +string Reason
        +string RazorpayRefundId
        +RefundStatus Status
        +DateTime ProcessedAt
    }

    class PaymentStatus {
        <<enumeration>>
        Pending
        Captured
        Failed
        Refunded
    }

    PaymentOrder "1" --> "*" PaymentRefund
    PaymentOrder --> PaymentStatus
```

### 6.2 Payment Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant GW as Gateway
    participant P as Payment Service
    participant RZ as Razorpay
    participant MQ as RabbitMQ
    participant S as Shipment Service

    C->>GW: POST /api/v1/payment/orders {shipmentId, amount, idempotencyKey}
    GW->>P: Forward
    P->>P: Check idempotency key (prevent duplicates)
    P->>RZ: POST /v1/orders {amount, currency, receipt}
    RZ-->>P: {razorpayOrderId}
    P->>P: Save PaymentOrder (Pending)
    P-->>C: {razorpayOrderId, razorpayKeyId, amount, currency}

    C->>C: Open Razorpay checkout modal
    C->>RZ: Customer completes payment
    RZ-->>C: {razorpay_order_id, razorpay_payment_id, razorpay_signature}

    C->>GW: POST /api/v1/payment/verify {orderId, paymentId, signature}
    GW->>P: Forward
    P->>P: HMAC-SHA256 verify signature
    alt Signature valid
        P->>P: Update PaymentOrder → Captured
        P->>MQ: Publish PaymentCaptured {shipmentId}
        MQ->>S: Consume → Update Shipment → Paid
        P-->>C: {success: true}
    else Signature invalid
        P->>P: Update PaymentOrder → Failed
        P-->>C: 400 Bad Request
    end
```

### 6.3 Signature Verification Algorithm

```
expectedSignature = HMAC-SHA256(razorpayOrderId + "|" + razorpayPaymentId, razorpayKeySecret)
isValid = (expectedSignature == razorpaySignature)
```

### 6.4 API Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | /api/v1/payment/orders | Bearer | Create payment order |
| POST | /api/v1/payment/verify | Bearer | Verify payment |
| GET | /api/v1/payment/shipments/{id} | Bearer | Get payments for shipment |
| POST | /api/v1/payment/webhook | None (HMAC) | Razorpay webhook |
| POST | /api/v1/payment/refund | Admin | Process refund |

---

## 7. Notification Service

### 7.1 Event Handlers

| Event | Action |
|---|---|
| ShipmentBooked | Email + SMS booking confirmation |
| PaymentCaptured | Email payment receipt |
| PaymentFailed | Email payment failure alert |
| ShipmentPickedUp | SMS pickup confirmation |
| ShipmentInTransit | Email in-transit update |
| ShipmentOutForDelivery | SMS out-for-delivery alert |
| ShipmentDelivered | Email + SMS delivery confirmation |
| ShipmentDelayed | Email delay notification |

### 7.2 Notification Delivery Flow

```mermaid
flowchart TD
    A["RabbitMQ Event"] --> B["Notification Consumer"]
    B --> C{"User Preferences"}
    C -->|Email enabled| D["SMTP Email Sender"]
    C -->|SMS enabled| E["Twilio SMS Sender"]
    D --> F["Log to NotificationHistory"]
    E --> F
    F --> G["Update Delivery Status"]
```

---

## 8. API Gateway — Routing Configuration

### 8.1 Route Map

| Upstream Path | Downstream Service | Port | Rate Limit |
|---|---|---|---|
| /api/v1/auth/** | Auth Service | 9001 | 20 req/min |
| /api/v1/shipment/** | Shipment Service | 9002 | 60 req/min |
| /api/v1/tracking/** | Tracking Service | 9003 | 100 req/min |
| /api/v1/notification/** | Notification Service | 9004 | 30 req/min |
| /api/v1/payment/** | Payment Service | 9005 | 20 req/min |

### 8.2 Circuit Breaker Configuration

```json
{
  "ExceptionsAllowedBeforeBreaking": 5,
  "DurationOfBreak": 30000,
  "TimeoutValue": 10000
}
```

---

## 9. Frontend Architecture

### 9.1 Angular Application Structure

```mermaid
graph TD
    APP["AppComponent"] --> ROUTER["Angular Router"]
    ROUTER --> LANDING["LandingComponent"]
    ROUTER --> AUTH["Auth Module<br/>Login / Register"]
    ROUTER --> DASH["DashboardComponent"]
    ROUTER --> WIZARD["ShipmentWizardComponent<br/>7 Steps"]
    ROUTER --> SHIPS["ShipmentsComponent"]
    ROUTER --> TRACK["TrackingComponent"]
    ROUTER --> CHECKOUT["CheckoutComponent"]
    ROUTER --> ADMIN["Admin Module<br/>Dashboard/Users/Hubs/Rates"]
    ROUTER --> SETTINGS["SettingsComponent"]
    ROUTER --> ADDR["AddressBookComponent"]

    WIZARD --> API["ApiService"]
    WIZARD --> PAY["PaymentService"]
    WIZARD --> STORE["StoreService"]

    API --> GW["API Gateway :8000"]
    PAY --> RZ["Razorpay SDK"]
    STORE --> LS["localStorage"]
```

### 9.2 State Management (StoreService Signals)

```mermaid
classDiagram
    class StoreService {
        -Signal~User?~ _user
        -Signal~AppNotification[]~ _notifications
        -Signal~string~ _theme
        -Signal~any[]~ _addresses
        -Signal~number~ _carbonSaved
        +Computed currentUser
        +Computed isAuthenticated
        +Computed userRoles
        +Computed isAdmin
        +Computed notifications
        +Computed theme
        +Computed isDarkMode
        +setUser(user)
        +logout()
        +addNotification(msg, type)
        +removeNotification(id)
        +toggleTheme()
        +setAddresses(addresses)
        +updateCarbonMetric(amount)
    }
```

### 9.3 Shipment Wizard Steps

| Step | Form | Key Fields |
|---|---|---|
| 1 | originForm | senderName, senderEmail, senderPhone, senderAddress, city, state, postalCode |
| 2 | destForm | receiverName, receiverEmail, receiverPhone, receiverAddress, city, state, postalCode |
| 3 | packageForm | weight, length, width, height, type |
| 4 | serviceForm | serviceId (Standard / Express / Overnight) |
| 5 | addonsForm | insurance, carbonOffset, fragile, signature |
| 6 | billingForm | paymentMethod, promoCode |
| 7 | — | Confirmation, label generation, payment |

### 9.4 HTTP Interceptors

```mermaid
graph LR
    REQ["HTTP Request"] --> AUTH_INT["AuthInterceptor<br/>Attach Bearer token"]
    AUTH_INT --> CORR_INT["CorrelationIdInterceptor<br/>Attach X-Correlation-Id"]
    CORR_INT --> GW["API Gateway"]
    GW --> ERR_INT["ErrorInterceptor<br/>Handle 401/403/500"]
    ERR_INT --> COMP["Component"]
```

### 9.5 Route Guards

| Guard | Condition | Redirect |
|---|---|---|
| authGuard | User must be authenticated | /login |
| noAuthGuard | User must NOT be authenticated | /dashboard |
| roleGuard | User must have specified role | /dashboard |

---

## 10. Database Schema Details

### 10.1 Auth Database — Key Tables

```mermaid
erDiagram
    Users {
        uniqueidentifier Id PK
        nvarchar Email UK
        nvarchar PasswordHash
        nvarchar FullName
        nvarchar PhoneNumber
        bit EmailVerified
        bit IsActive
        bit IsLocked
        int FailedLoginAttempts
        datetime2 LastLoginAt
        datetime2 CreatedAt
        uniqueidentifier CreatedBy
    }
    RefreshTokens {
        uniqueidentifier Id PK
        uniqueidentifier UserId FK
        nvarchar Token UK
        datetime2 ExpiresAt
        bit IsRevoked
    }
    UserRoles {
        uniqueidentifier Id PK
        uniqueidentifier UserId FK
        nvarchar Role
    }
    UserAddresses {
        uniqueidentifier Id PK
        uniqueidentifier UserId FK
        nvarchar Label
        nvarchar ContactName
        nvarchar City
        nvarchar State
        bit IsDefault
    }
    Users ||--o{ RefreshTokens : has
    Users ||--o{ UserRoles : has
    Users ||--o{ UserAddresses : has
```

### 10.2 Shipment Database — Key Tables

```mermaid
erDiagram
    Shipments {
        uniqueidentifier Id PK
        nvarchar TrackingNumber UK
        uniqueidentifier CustomerId
        int Status
        uniqueidentifier SenderAddressId FK
        uniqueidentifier ReceiverAddressId FK
        uniqueidentifier ServiceRateId FK
        decimal ActualWeight
        decimal ChargeableWeight
        decimal TotalCost
        nvarchar Currency
        datetime2 EstimatedDeliveryDate
        nvarchar IdempotencyKey UK
    }
    Addresses {
        uniqueidentifier Id PK
        nvarchar ContactName
        nvarchar City
        nvarchar State
        nvarchar PostalCode
    }
    ServiceRates {
        uniqueidentifier Id PK
        nvarchar ServiceType
        decimal BaseRatePerKg
        decimal MinimumCharge
        int EstimatedDeliveryDays
        bit IsActive
    }
    ShipmentItems {
        uniqueidentifier Id PK
        uniqueidentifier ShipmentId FK
        nvarchar Description
        int Quantity
        decimal Weight
    }
    Shipments ||--|| Addresses : SenderAddress
    Shipments ||--|| Addresses : ReceiverAddress
    Shipments ||--|| ServiceRates : uses
    Shipments ||--o{ ShipmentItems : contains
```

### 10.3 Payment Database — Key Tables

```mermaid
erDiagram
    PaymentOrders {
        uniqueidentifier Id PK
        uniqueidentifier ShipmentId
        nvarchar TrackingNumber
        nvarchar RazorpayOrderId UK
        nvarchar RazorpayPaymentId
        decimal Amount
        nvarchar Currency
        int Status
        datetime2 CapturedAt
        nvarchar IdempotencyKey UK
    }
    PaymentRefunds {
        uniqueidentifier Id PK
        uniqueidentifier PaymentOrderId FK
        decimal Amount
        nvarchar Reason
        nvarchar RazorpayRefundId
        int Status
    }
    PaymentOrders ||--o{ PaymentRefunds : has
```

---

## 11. Error Handling Strategy

| Layer | Mechanism |
|---|---|
| Frontend | ErrorInterceptor catches HTTP errors, StoreService shows toast notifications |
| Gateway | Returns 4xx/5xx with structured error body |
| Services | Global exception middleware returns `{error: "message"}` JSON |
| Domain | Domain exceptions for business rule violations |
| Infrastructure | EF Core retry policies for transient DB failures |
| Messaging | Dead-letter queues for failed RabbitMQ message processing |
