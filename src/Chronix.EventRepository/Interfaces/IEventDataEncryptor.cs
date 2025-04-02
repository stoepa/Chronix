using Chronix.EventRepository.Types;

namespace Chronix.EventRepository.Interfaces;

public interface IEventDataEncryptor
{
    public DomainEvent Encrypt(DomainEvent domainEvent);
    public DomainEvent Decrypt(DomainEvent domainEvent);
}
