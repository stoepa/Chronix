using Chronix.EventRepository.Interfaces;
using Chronix.EventRepository.Types;

namespace Chronix.EventRepository.Encryptors;

public class NoEncryptionEncrypter : IEventDataEncryptor
{
    public DomainEvent Decrypt(DomainEvent domainEvent)
    {
        return domainEvent;
    }

    public DomainEvent Encrypt(DomainEvent domainEvent)
    {
        return domainEvent;
    }
}
