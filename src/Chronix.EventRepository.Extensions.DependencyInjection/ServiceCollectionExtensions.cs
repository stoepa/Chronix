using Chronix.EventRepository.Extensions.DependencyInjection.Interfaces;
using Chronix.EventRepository.Registry;
using Chronix.EventRepository.Types;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Chronix.EventRepository.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddEventRepository<T>(this IServiceCollection services, string streamName) where T : AggregateRoot {
		services.AddSingleton(sp => {
			var eventStoreClient = sp.GetRequiredService<EventStoreClient>();
			var stage = EventRepositoryBuilder<T>.Create(eventStoreClient, streamName, sp);
			EventRepositoryBuilder<T>.Create(eventStoreClient, streamName, sp);
			var eventRepository = stage.Build();
			return eventRepository;
		});

		return services;
	}

	public static IServiceCollection AddEventRepository<T>(this IServiceCollection services, string streamName, Type assemblyToScan) where T : AggregateRoot
    {
        services.AddSingleton(sp =>
        {
            var eventStoreClient = sp.GetRequiredService<EventStoreClient>();
            var stage = EventRepositoryBuilder<T>.Create(eventStoreClient, streamName, sp);
			EventRepositoryBuilder<T>.Create(eventStoreClient, streamName, sp);
            var eventRepository = stage.Build();
            return eventRepository;
        });
        
        services.AddProjectionsFromAssemblyContaining(assemblyToScan);
        
        return services;
    }

	public static IServiceCollection AddEventRepository<T>(this IServiceCollection services, string streamName, Action<IServiceProvider, IConfigureEventStoreClientStage<T>> builder, Type assemblyToScan) where T : AggregateRoot {
		services.AddSingleton(sp => {
			var eventStoreClient = sp.GetRequiredService<EventStoreClient>();
			var stage = EventRepositoryBuilder<T>.Create(eventStoreClient, streamName, sp);
			builder(sp, stage);
			var eventRepository = stage.Build();
			return eventRepository;
		});

		services.AddProjectionsFromAssemblyContaining(assemblyToScan);

		return services;
	}
}
