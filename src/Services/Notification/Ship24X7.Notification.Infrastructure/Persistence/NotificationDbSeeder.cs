using Microsoft.EntityFrameworkCore;
using Ship24X7.Notification.Domain.Entities;
using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Infrastructure.Persistence;

public static class NotificationDbSeeder
{
    public static async Task SeedAsync(NotificationDbContext context)
    {
        await context.Database.MigrateAsync();

        // Seed base templates once (idempotent — skipped if any already exist)
        if (!await context.NotificationTemplates.AnyAsync())
        {
            var templates = new List<NotificationTemplate>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "BookingConfirmation",
                    Type = TemplateType.BookingConfirmation,
                    Channel = NotificationChannel.Email,
                    Subject = "Shipment Booking Confirmed - {{TrackingNumber}}",
                    BodyTemplate = "Dear {{CustomerName}},\n\nYour shipment has been successfully booked!\n\nTracking Number: {{TrackingNumber}}\nService Type: {{ServiceType}}\nTotal Cost: {{TotalCost}}\nEstimated Delivery: {{EstimatedDeliveryDate}}\n\nYou can track your shipment at: {{TrackingUrl}}\n\nThank you for choosing Ship24X7!\n\nBest regards,\nShip24X7 Team",
                    RequiredPlaceholders = new[] { "CustomerName", "TrackingNumber", "ServiceType", "TotalCost", "EstimatedDeliveryDate", "TrackingUrl" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "DeliveryConfirmation",
                    Type = TemplateType.DeliveryConfirmation,
                    Channel = NotificationChannel.Email,
                    Subject = "Shipment Delivered - {{TrackingNumber}}",
                    BodyTemplate = "Dear {{CustomerName}},\n\nGreat news! Your shipment has been delivered successfully.\n\nTracking Number: {{TrackingNumber}}\nDelivered On: {{DeliveryDate}}\nReceived By: {{ReceivedBy}}\n\nThank you for using Ship24X7!\n\nBest regards,\nShip24X7 Team",
                    RequiredPlaceholders = new[] { "CustomerName", "TrackingNumber", "DeliveryDate", "ReceivedBy" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "DeliveryConfirmationSMS",
                    Type = TemplateType.DeliveryConfirmation,
                    Channel = NotificationChannel.SMS,
                    Subject = "",
                    BodyTemplate = "Ship24X7: Your shipment {{TrackingNumber}} has been delivered. Received by: {{ReceivedBy}}. Thank you!",
                    RequiredPlaceholders = new[] { "TrackingNumber", "ReceivedBy" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "PaymentConfirmation",
                    Type = TemplateType.PaymentConfirmation,
                    Channel = NotificationChannel.Email,
                    Subject = "Payment Confirmed - {{TrackingNumber}}",
                    BodyTemplate = "Dear {{CustomerName}},\n\nYour payment has been successfully processed.\n\nTracking Number: {{TrackingNumber}}\nPayment Amount: {{PaymentAmount}}\nPayment ID: {{PaymentId}}\nPayment Date: {{PaymentDate}}\n\nYour shipment will be processed shortly.\n\nBest regards,\nShip24X7 Team",
                    RequiredPlaceholders = new[] { "CustomerName", "TrackingNumber", "PaymentAmount", "PaymentId", "PaymentDate" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "ShipmentDelayed",
                    Type = TemplateType.ShipmentDelayed,
                    Channel = NotificationChannel.Email,
                    Subject = "Shipment Delayed - {{TrackingNumber}}",
                    BodyTemplate = "Dear {{CustomerName}},\n\nWe regret to inform you that your shipment has been delayed.\n\nTracking Number: {{TrackingNumber}}\nReason: {{DelayReason}}\nNew Estimated Delivery: {{NewEstimatedDeliveryDate}}\n\nWe apologize for the inconvenience.\n\nBest regards,\nShip24X7 Team",
                    RequiredPlaceholders = new[] { "CustomerName", "TrackingNumber", "DelayReason", "NewEstimatedDeliveryDate" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "PasswordReset",
                    Type = TemplateType.PasswordReset,
                    Channel = NotificationChannel.Email,
                    Subject = "Password Reset Request - Ship24X7",
                    BodyTemplate = "Dear {{UserName}},\n\nWe received a request to reset your password.\n\nClick the link below to reset your password:\n{{ResetLink}}\n\nThis link will expire in 1 hour.\n\nIf you did not request this, please ignore this email.\n\nBest regards,\nShip24X7 Team",
                    RequiredPlaceholders = new[] { "UserName", "ResetLink" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "EmailVerification",
                    Type = TemplateType.EmailVerification,
                    Channel = NotificationChannel.Email,
                    Subject = "Verify Your Email - Ship24X7",
                    BodyTemplate = "Dear {{UserName}},\n\nWelcome to Ship24X7!\n\nPlease verify your email address by clicking the link below:\n{{VerificationLink}}\n\nThis link will expire in 24 hours.\n\nBest regards,\nShip24X7 Team",
                    RequiredPlaceholders = new[] { "UserName", "VerificationLink" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "PickupScheduled",
                    Type = TemplateType.PickupScheduled,
                    Channel = NotificationChannel.Email,
                    Subject = "Pickup Scheduled - {{TrackingNumber}}",
                    BodyTemplate = "Dear {{CustomerName}},\n\nYour pickup has been scheduled successfully.\n\nTracking Number: {{TrackingNumber}}\nPickup Date: {{PickupDate}}\nPickup Time Slot: {{PickupTimeSlot}}\nPickup Address: {{PickupAddress}}\n\nPlease ensure the package is ready for pickup.\n\nBest regards,\nShip24X7 Team",
                    RequiredPlaceholders = new[] { "CustomerName", "TrackingNumber", "PickupDate", "PickupTimeSlot", "PickupAddress" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.NotificationTemplates.AddRangeAsync(templates);
            await context.SaveChangesAsync();
        }

        // Seed ShipmentOutForDelivery email template (fixed ID — safe to run on existing DBs)
        var outForDeliveryEmailId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == outForDeliveryEmailId))
        {
            context.NotificationTemplates.Add(new()
            {
                Id = outForDeliveryEmailId,
                Name = "ShipmentOutForDelivery",
                Type = TemplateType.ShipmentOutForDelivery,
                Channel = NotificationChannel.Email,
                Subject = "Out for Delivery - {{TrackingNumber}}",
                BodyTemplate = "Dear Customer,\n\nYour shipment {{TrackingNumber}} is out for delivery!\n\nOTP: {{DeliveryOtp}}\nAgent: {{DeliveryAgent}}\nExpires: {{OtpExpiresAt}}",
                RequiredPlaceholders = new[] { "TrackingNumber", "DeliveryOtp", "DeliveryAgent", "OtpExpiresAt" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Seed ShipmentOutForDelivery SMS template (fixed ID — safe to run on existing DBs)
        var outForDeliverySmsId = Guid.Parse("88888888-8888-8888-8888-888888888888");
        if (!await context.NotificationTemplates.AnyAsync(t => t.Id == outForDeliverySmsId))
        {
            context.NotificationTemplates.Add(new()
            {
                Id = outForDeliverySmsId,
                Name = "ShipmentOutForDeliverySMS",
                Type = TemplateType.ShipmentOutForDelivery,
                Channel = NotificationChannel.SMS,
                Subject = "",
                BodyTemplate = "Ship24X7: Shipment {{TrackingNumber}} is out for delivery. OTP: {{DeliveryOtp}}. Agent: {{DeliveryAgent}}.",
                RequiredPlaceholders = new[] { "TrackingNumber", "DeliveryOtp", "DeliveryAgent" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
    }
}
