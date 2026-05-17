using MediatR;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command to soft-delete a saved address from the user's address book.
/// </summary>
public class DeleteAddressCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public Guid AddressId { get; set; }
}
