namespace Chronix.EventRepository.Types;

public record EventHandler(Type Type, Action<DomainEvent> When);
