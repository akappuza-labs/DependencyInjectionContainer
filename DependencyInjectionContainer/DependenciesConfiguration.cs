using System.Collections.Concurrent;

namespace DependencyInjectionContainer;

public class DependenciesConfiguration
{
    private readonly ConcurrentDictionary<Type, ConcurrentBag<ImplementationDescription>> _lookupTable;

    public DependenciesConfiguration()
    {
        _lookupTable = new ConcurrentDictionary<Type, ConcurrentBag<ImplementationDescription>>();
    }

    public void Register(Type dependency, Type implementation, Enum? id = null,
        Lifecycle lifecycle = Lifecycle.Transient)
    {
        _lookupTable.TryAdd(dependency, new ConcurrentBag<ImplementationDescription>());
        
        _lookupTable[dependency].Add(new ImplementationDescription(id, implementation, lifecycle));
    }

    public void Register<TDependency, TImplementation>(Enum? id = null, Lifecycle lifecycle = Lifecycle.Transient)
    {
        var typeDependency = typeof(TDependency);
        if (typeDependency == null) 
            throw new DependenciesConfigurationException($"typeof({nameof(typeDependency)}) returns null");
        
        var typeImplementation = typeof(TImplementation);
        if (typeImplementation == null)
            throw new DependenciesConfigurationException($"typeof({nameof(typeImplementation)}) returns null");

        Register(typeDependency, typeImplementation, id, lifecycle);
    }

    public List<ImplementationDescription> GetImplementationsDescriptions<TDependency>()
    {
        return GetImplementationsDescriptions(typeof(TDependency));
    }
    
    public List<ImplementationDescription> GetImplementationsDescriptions(Type dependency)
    {
        if (!_lookupTable.TryGetValue(dependency, out var implDescriptions))
            throw new DependenciesConfigurationException(
                $"Configuration hasn't implementation for {nameof(dependency)}");
        
        return implDescriptions.ToList();
    }

    public ImplementationDescription GetImplementationDescription<TDependency>(Enum id)
    {
        return GetImplementationDescription(typeof(TDependency), id);
    }
    public ImplementationDescription GetImplementationDescription(Type dependency, Enum id)
    {
        if (!_lookupTable.TryGetValue(dependency, out var implDescriptions))
            throw new DependenciesConfigurationException(
                $"Configuration hasn't implementation for {nameof(dependency)}");

        try
        {
            return implDescriptions.First(des => des.Id != null && des.Id.Equals(id));
        }
        catch (InvalidOperationException)
        {
            throw new DependenciesConfigurationException(
                $"Configuration hasn't implementation for {nameof(dependency)} with id {id}");
        }
    }

    public List<Type> GetAllDependencies()
    {
        return _lookupTable.Keys.ToList();
    }

    public bool IsContainsDependency(Type dependency)
    {
        return _lookupTable.ContainsKey(dependency);
    }
}