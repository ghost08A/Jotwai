namespace Jotwai.Domain.Interfaces;

/// <summary>
/// Marker for an in-process Domain Event (ADR-0002) - the only way Modules may
/// notify each other of a state change.
/// </summary>
public interface IDomainEvent;
