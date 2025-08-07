namespace AbyssIrc.Server.Core.Data.Internal.Services;

public record struct ServiceDefinitionObject(Type ServiceType, Type ImplementationType, int Priority = 0);
