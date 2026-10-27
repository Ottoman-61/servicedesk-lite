namespace ServiceDesk.Api.Models;

public record CreateTicketDto(string Title, string? Description, string? Priority, int? CreatedBy, int? DeviceId);
public record UpdateTicketDto(string? Status, string? Priority, int? AssignedTo, string? SolutionNote);