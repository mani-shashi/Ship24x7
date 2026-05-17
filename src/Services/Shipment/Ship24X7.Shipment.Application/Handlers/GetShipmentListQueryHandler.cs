using MediatR;
using Ship24X7.Shipment.Application.DTOs;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Application.Queries;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Handler for retrieving paginated list of shipments with filters.
/// Returns full shipment details including items, addresses, and service rates.
/// </summary>
public class GetShipmentListQueryHandler : IRequestHandler<GetShipmentListQuery, List<ShipmentResponse>>
{
    private readonly IShipmentRepository _shipmentRepository;

    public GetShipmentListQueryHandler(IShipmentRepository shipmentRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    public async Task<List<ShipmentResponse>> Handle(GetShipmentListQuery request, CancellationToken cancellationToken)
    {
        var shipments = await _shipmentRepository.GetListAsync(
            request.CustomerId,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.Skip,
            request.Take);

        return shipments.Select(MapToResponse).ToList();
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
            CreatedAt = shipment.CreatedAt,
            PickupId = shipment.Pickup?.Id,
            CurrentHubId = shipment.CurrentHubId
        };
    }
}
