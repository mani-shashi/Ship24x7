using MediatR;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.DTOs;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Entities;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Handler for processing CreateShipment requests. Implements business logic and coordinates with repositories and services.
/// </summary>
public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, ShipmentResponse>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IServiceRateRepository _serviceRateRepository;
    private readonly ITrackingNumberGenerator _trackingNumberGenerator;
    private readonly IRateCalculationService _rateCalculationService;

    public CreateShipmentCommandHandler(
        IShipmentRepository shipmentRepository,
        IAddressRepository addressRepository,
        IServiceRateRepository serviceRateRepository,
        ITrackingNumberGenerator trackingNumberGenerator,
        IRateCalculationService rateCalculationService)
    {
        _shipmentRepository = shipmentRepository;
        _addressRepository = addressRepository;
        _serviceRateRepository = serviceRateRepository;
        _trackingNumberGenerator = trackingNumberGenerator;
        _rateCalculationService = rateCalculationService;
    }

    public async Task<ShipmentResponse> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        // Check for idempotency
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existingShipment = await _shipmentRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey);
            if (existingShipment != null)
            {
                return MapToResponse(existingShipment.Shipment);
            }
        }

        // Get service rate
        var serviceRate = await _serviceRateRepository.GetByIdAsync(request.ServiceRateId);
        if (serviceRate == null || !serviceRate.IsActive)
            throw new InvalidOperationException("Invalid or inactive service rate");

        // Create sender address
        var senderAddress = new Address
        {
            Id = Guid.NewGuid(),
            ContactName = request.SenderContactName,
            ContactPhone = request.SenderContactPhone,
            ContactEmail = request.SenderContactEmail,
            AddressLine1 = request.SenderAddressLine1,
            AddressLine2 = request.SenderAddressLine2,
            City = request.SenderCity,
            State = request.SenderState,
            PostalCode = request.SenderPostalCode,
            Country = request.SenderCountry,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CustomerId
        };
        await _addressRepository.AddAsync(senderAddress);

        // Create receiver address
        var receiverAddress = new Address
        {
            Id = Guid.NewGuid(),
            ContactName = request.ReceiverContactName,
            ContactPhone = request.ReceiverContactPhone,
            ContactEmail = request.ReceiverContactEmail,
            AddressLine1 = request.ReceiverAddressLine1,
            AddressLine2 = request.ReceiverAddressLine2,
            City = request.ReceiverCity,
            State = request.ReceiverState,
            PostalCode = request.ReceiverPostalCode,
            Country = request.ReceiverCountry,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CustomerId
        };
        await _addressRepository.AddAsync(receiverAddress);

        // Calculate total weight and dimensions
        var totalWeight = request.Items.Sum(i => i.Weight * i.Quantity);
        var maxLength = request.Items.Max(i => i.Length);
        var maxWidth = request.Items.Max(i => i.Width);
        var maxHeight = request.Items.Max(i => i.Height);

        // Calculate rates
        var rates = await _rateCalculationService.CalculateRatesAsync(
            totalWeight, maxLength, maxWidth, maxHeight, 
            serviceRate.ServiceType, request.DeclaredValue);
        
        var selectedRate = rates.FirstOrDefault(r => r.ServiceRateId == request.ServiceRateId);
        if (selectedRate == null)
            throw new InvalidOperationException("Rate calculation failed");

        // Generate tracking number
        var trackingNumber = await _trackingNumberGenerator.GenerateAsync();

        // Create shipment
        var shipment = new Domain.Entities.Shipment
        {
            Id = Guid.NewGuid(),
            TrackingNumber = trackingNumber,
            CustomerId = request.CustomerId,
            Status = ShipmentStatus.Draft,
            SenderAddressId = senderAddress.Id,
            ReceiverAddressId = receiverAddress.Id,
            ServiceRateId = request.ServiceRateId,
            ActualWeight = selectedRate.ActualWeight,
            VolumetricWeight = selectedRate.VolumetricWeight,
            ChargeableWeight = selectedRate.ChargeableWeight,
            BaseRate = selectedRate.BaseRate,
            FuelSurcharge = selectedRate.FuelSurcharge,
            InsuranceCost = selectedRate.InsuranceCost,
            TotalCost = selectedRate.TotalCost,
            Currency = selectedRate.Currency,
            EstimatedDeliveryDate = selectedRate.EstimatedDeliveryDate,
            IsFragile = request.IsFragile,
            RequiresRefrigeration = request.RequiresRefrigeration,
            DeclaredValue = request.DeclaredValue,
            IdempotencyKey = request.IdempotencyKey,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CustomerId
        };

        // Add items
        foreach (var itemDto in request.Items)
        {
            var item = new ShipmentItem
            {
                Id = Guid.NewGuid(),
                ShipmentId = shipment.Id,
                Description = itemDto.Description,
                Quantity = itemDto.Quantity,
                Weight = itemDto.Weight,
                Length = itemDto.Length,
                Width = itemDto.Width,
                Height = itemDto.Height,
                PackageType = itemDto.PackageType,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CustomerId
            };
            shipment.Items.Add(item);
        }

        await _shipmentRepository.AddAsync(shipment);

        return MapToResponse(shipment);
    }

    private ShipmentResponse MapToResponse(Domain.Entities.Shipment shipment)
    {
        return new ShipmentResponse
        {
            Id = shipment.Id,
            TrackingNumber = shipment.TrackingNumber,
            CustomerId = shipment.CustomerId,
            Status = shipment.Status,
            SenderAddress = new AddressDto
            {
                Id = shipment.SenderAddress.Id,
                ContactName = shipment.SenderAddress.ContactName,
                ContactPhone = shipment.SenderAddress.ContactPhone,
                ContactEmail = shipment.SenderAddress.ContactEmail,
                AddressLine1 = shipment.SenderAddress.AddressLine1,
                AddressLine2 = shipment.SenderAddress.AddressLine2,
                City = shipment.SenderAddress.City,
                State = shipment.SenderAddress.State,
                PostalCode = shipment.SenderAddress.PostalCode,
                Country = shipment.SenderAddress.Country,
                Latitude = shipment.SenderAddress.Latitude,
                Longitude = shipment.SenderAddress.Longitude
            },
            ReceiverAddress = new AddressDto
            {
                Id = shipment.ReceiverAddress.Id,
                ContactName = shipment.ReceiverAddress.ContactName,
                ContactPhone = shipment.ReceiverAddress.ContactPhone,
                ContactEmail = shipment.ReceiverAddress.ContactEmail,
                AddressLine1 = shipment.ReceiverAddress.AddressLine1,
                AddressLine2 = shipment.ReceiverAddress.AddressLine2,
                City = shipment.ReceiverAddress.City,
                State = shipment.ReceiverAddress.State,
                PostalCode = shipment.ReceiverAddress.PostalCode,
                Country = shipment.ReceiverAddress.Country,
                Latitude = shipment.ReceiverAddress.Latitude,
                Longitude = shipment.ReceiverAddress.Longitude
            },
            ServiceRate = new ServiceRateDto
            {
                Id = shipment.ServiceRate.Id,
                ServiceType = shipment.ServiceRate.ServiceType,
                ServiceName = shipment.ServiceRate.ServiceName,
                EstimatedDeliveryDays = shipment.ServiceRate.EstimatedDeliveryDays
            },
            Items = shipment.Items.Select(i => new ShipmentItemResponseDto
            {
                Id = i.Id,
                Description = i.Description,
                Quantity = i.Quantity,
                Weight = i.Weight,
                Length = i.Length,
                Width = i.Width,
                Height = i.Height,
                PackageType = i.PackageType
            }).ToList(),
            ActualWeight = shipment.ActualWeight,
            VolumetricWeight = shipment.VolumetricWeight,
            ChargeableWeight = shipment.ChargeableWeight,
            BaseRate = shipment.BaseRate,
            FuelSurcharge = shipment.FuelSurcharge,
            InsuranceCost = shipment.InsuranceCost,
            TotalCost = shipment.TotalCost,
            Currency = shipment.Currency,
            EstimatedDeliveryDate = shipment.EstimatedDeliveryDate,
            ActualDeliveryDate = shipment.ActualDeliveryDate,
            IsFragile = shipment.IsFragile,
            RequiresRefrigeration = shipment.RequiresRefrigeration,
            DeclaredValue = shipment.DeclaredValue,
            CreatedAt = shipment.CreatedAt
        };
    }
}
