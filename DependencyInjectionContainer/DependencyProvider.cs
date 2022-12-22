using System.Collections;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Net.WebSockets;
using System.Reflection;

namespace DependencyInjectionContainer;

public class DependencyProvider
{
    private readonly DependenciesConfiguration _configuration;

    private readonly ConcurrentDictionary<ImplementationDescription, object> _singletons;

    public DependencyProvider(DependenciesConfiguration configuration)
    {
        if (!IsValidConfig(configuration))
            throw new DependenciesProviderException("Not valid configuration");
            
        _configuration = configuration;
        _singletons = new ConcurrentDictionary<ImplementationDescription, object>();
    }
    
    public TDependency Resolve<TDependency>(Enum? id = null)
    {
        return (TDependency)Resolve(typeof(TDependency), id);
    }
    
    public object Resolve(Type dependency, Enum? id = null)
    {
        try
        {
            if (dependency.IsGenericType && dependency.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return GetImplementationList(dependency);
            }

            ImplementationDescription implementation;

            if (dependency.IsGenericType)
            {
                if (id is null)
                    implementation = _configuration.GetImplementationsDescriptions(dependency.GetGenericTypeDefinition())[0];
                else
                    implementation = _configuration.GetImplementationsDescriptions(dependency.GetGenericTypeDefinition())
                        .First(impl => impl.Id != null && impl.Id.Equals(id));
                
                implementation.Type = implementation.Type.MakeGenericType(dependency.GenericTypeArguments);
            }
            else
            {
                if (id is null)
                    implementation = _configuration.GetImplementationsDescriptions(dependency)[0];
                else
                    implementation = _configuration.GetImplementationsDescriptions(dependency)
                        .First(impl => impl.Id != null && impl.Id.Equals(id));
            }

            
           
            
            return implementation.Lifecycle == Lifecycle.Singleton 
                ? GetSingleton(implementation)
                : GetTransient(implementation.Type);
        }
        catch (DependenciesConfigurationException ex)
        {
            throw new DependenciesProviderException("Couldn't get implementation", ex);
        }
    }
    
    private static bool IsValidConfig(DependenciesConfiguration configs)
    {
        var dependencies = configs.GetAllDependencies();
        
        return 
            !(from dependency in dependencies 
                from implementation in configs.GetImplementationsDescriptions(dependency) 
                where !IsValidImplementation(dependency, implementation.ToType()) select dependency).Any();
    }

    private static bool IsValidImplementation(Type dependency, Type implementation)
    {
        return !(implementation.IsAbstract || implementation.IsInterface)
               && (implementation.IsGenericTypeDefinition || implementation.IsAssignableTo(dependency));
    }

    private object? CreateInstance(Type type)
    {
        var ctor = GetConstructor(type);

        if (ctor is null) return Activator.CreateInstance(type);

        var parameters = ctor.GetParameters();
        var myParameters = new object[parameters.Length];
        var currParam = 0;
        
        foreach (var parameter in parameters)
        {
            myParameters[currParam] = Resolve(parameter.ParameterType);
            currParam += 1;
        }

        return ctor.Invoke(myParameters);
    }

    private ConstructorInfo? GetConstructor(Type type)
    {
        return type.GetConstructors()
            .FirstOrDefault(
                ctor => ctor
                    .GetParameters()
                    .All(parameter => _configuration.IsContainsDependency(parameter.ParameterType)));
    }

    private object GetImplementationList(Type dependency)
    {
        var genericType = dependency.GenericTypeArguments[0];
        var implementations = _configuration.GetImplementationsDescriptions(genericType);

        var genericListType = typeof(List<>).MakeGenericType(genericType);
        var list = Activator.CreateInstance(genericListType);

        if (list is null)
            throw new DependenciesProviderException("Couldn't create implementations list");

        var result = (IList)list;

        foreach (var obj in implementations.Select(impl => CreateInstance(impl.ToType())))
        {
            if (obj is null)
                throw new DependenciesProviderException("Couldn't create object");

            result.Add(obj);
        }

        return result;
    }

    private object GetTransient(Type type)
    {
        var impl = CreateInstance(type);

        if (impl is null)
            throw new DependenciesProviderException("Couldn't create object");

        return impl;
    }

    private object GetSingleton(ImplementationDescription implDescription)
    {
        if (_singletons.ContainsKey(implDescription))
            return _singletons[implDescription];
        
        var singleton =  GetTransient(implDescription.Type);
        _singletons[implDescription] = singleton;

        return singleton;
    }
}