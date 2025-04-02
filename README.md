# Chronix

**Chronix** is a modern .NET library that brings **DDD-style aggregates**, the **Repository pattern**, and **Projections** to [EventStoreDB](https://eventstore.com/). It offers a clean and extensible way to build event-sourced systems with a focus on maintainability and developer experience.

---

## ✨ Features

- ✅ Clean **Repository pattern** over EventStoreDB
- ✅ Easily define **aggregates**, **entities** and **value objects** using Domain-Driven Design (DDD) principles
- ✅ Append and retrieve events as aggregates with minimal boilerplate
- ✅ Define **projections** alongside your domain
- ✅ Pluggable serialization and stream naming

---

## 🚀 Getting Started

### 1. Install the NuGet Package

```bash
dotnet add package Chronix.EventRepository
```

## 🛠️ Configuring Dependency Injection

Chronix provides a simple way to register your aggregates and projections using the built-in `IServiceCollection` extensions.

Make sure you install the DI package:

```bash
dotnet add package Chronix.Extensions.DependencyInjection
```

### 2.1 ✅ Add an Event Repository
Registers an event repository for your aggregate, using the given stream prefix.

```bash
builder.Services.AddEventRepository<MyAggregate>("Member");
```

### 2.2 ✅ Add Repository + Scan for Projections
Scans the given assembly for any inline projections (e.g., read models or handlers) and wires them automatically.

```bash
builder.Services.AddEventRepository<MyAggregate>("Member", typeof(ProjectionAssemblyToScan));
```

### 2.2 ✅ Full Control: Configure the Builder
Gives you full access to the builder to register serializers, encrypters, metadata enrichers and other options.

```bash
builder.Services.AddEventRepository<MyAggregate>(
    "Member",
    typeof(ProjectionAssemblyToScan),
    (serviceProvider, builder) =>
    {
        builder.Encryption(new NoEncryptionEncrypter())
                .MetadataEnricher(new BasicMetadataEnricher())
                .Serializer(new AggregateRootSerializer())
                .Options(new EventRepositoryOptions
                {
                    AutoRevisionAfterNthEvent = 100
                });
    });
```




