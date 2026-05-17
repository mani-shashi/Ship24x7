using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Interfaces;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Soft-deletes a saved address after verifying ownership.
/// </summary>
public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, Unit>
{
    private readonly IUserAddressRepository _addressRepository;

    public DeleteAddressCommandHandler(IUserAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<Unit> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByIdAsync(request.AddressId)
            ?? throw new InvalidOperationException("Address not found");

        if (address.UserId != request.UserId)
            throw new UnauthorizedAccessException("Address does not belong to this user");

        await _addressRepository.DeleteAsync(request.AddressId);

        return Unit.Value;
    }
}
