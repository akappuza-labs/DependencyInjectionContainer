using System.Collections.Generic;
using System.Linq;
using DependencyInjectionContainer;
using NUnit.Framework;

namespace Tests;

public class DependencyInjectionContainerTests
{
    private DependenciesConfiguration _configuration = null!;
    private DependencyProvider _provider = null!;

    [SetUp]
    public void Setup()
    {
        _configuration = new DependenciesConfiguration();
    }

    [Test]
    public void CreateInvalidConfigurations()
    {
        _configuration.Register<IEnumerable<int>, int>();

        try
        {
            _provider = new DependencyProvider(_configuration);
        }
        catch (DependenciesProviderException)
        {
            Assert.Pass();
        }

        Assert.Fail("Incorrect work with invalid configuration");
    }
    
    [Test]
    public void CreateValidConfigurations()
    {
        _configuration.Register<InputData.IService, InputData.Service1>();

        _provider = new DependencyProvider(_configuration);
        
        Assert.IsNotNull(_provider, "Incorrect work with invalid configuration");
    }
    
    [Test]
    public void CreateValidConfigurationsWithObject()
    {
        _configuration.Register<object, InputData.Service1>();

        _provider = new DependencyProvider(_configuration);
        
        Assert.IsNotNull(_provider, "Incorrect work with invalid configuration");
    }
    
    [Test]
    public void CreateImplementationAsAbstractClass()
    {
        _configuration.Register<InputData.AbstractService, InputData.AbstractService>();

        try
        {
            _provider = new DependencyProvider(_configuration);
        }
        catch (DependenciesProviderException)
        {
            Assert.Pass();
        }

        Assert.Fail("Incorrect work with invalid configuration");
    }
    
    [Test]
    public void CreateImplementationAsInterface()
    {
        _configuration.Register<InputData.IService, InputData.IService>();

        try
        {
            _provider = new DependencyProvider(_configuration);
        }
        catch (DependenciesProviderException)
        {
            Assert.Pass();
        }

        Assert.Fail("Incorrect work with invalid configuration");
    }

    [Test]
    public void GetImplementation()
    {
        _configuration.Register<InputData.IService, InputData.Service1>();
        _configuration.Register<InputData.AbstractService, InputData.Service2>();

        _provider = new DependencyProvider(_configuration);

        var actual = _provider.Resolve<InputData.IService>().GetName();

        var excepted = "Service1";
        
        Assert.AreEqual(excepted, actual, $"Resolve returns '{actual}'");
    }
    
    [Test]
    public void GetHierarchicalImplementations()
    {
        _configuration.Register<InputData.IService, InputData.ServiceImpl>();
        _configuration.Register<InputData.IRepository, InputData.RepositoryImpl>();

        _provider = new DependencyProvider(_configuration);

        var actual = _provider.Resolve<InputData.IService>().GetName();

        var excepted = "ServiceImpl <- RepositoryImpl";
        
        Assert.AreEqual(excepted, actual, $"Resolve returns '{actual}'");
    }

    [Test]
    public void GetManyImplementations()
    {
        _configuration.Register<InputData.IService, InputData.Service1>();
        _configuration.Register<InputData.IService, InputData.Service2>();
        _configuration.Register<InputData.IService, InputData.ServiceImpl>();
        _configuration.Register<InputData.IRepository, InputData.RepositoryImpl>();

        _provider = new DependencyProvider(_configuration);

        var actual = _provider.Resolve<IEnumerable<InputData.IService>>()
            .Select(s => s.GetName()).ToArray();

        var isCorrect = actual.Contains("Service1") 
                        && actual.Contains("Service2")
                        && actual.Contains("ServiceImpl <- RepositoryImpl");
        
        Assert.IsTrue(isCorrect, $"Resolve returns invalid value");
    }
    
    [Test]
    public void CreateSingleton()
    {
        InputData.Counter.count = 0;
        _configuration.Register<InputData.Counter, InputData.Counter>(null, Lifecycle.Singleton);

        _provider = new DependencyProvider(_configuration);
        
        _provider.Resolve<InputData.Counter>();
        _provider.Resolve<InputData.Counter>();
        _provider.Resolve<InputData.Counter>();
        var actual = _provider.Resolve<InputData.Counter>().Count;
        var excepted = 1;
        
        Assert.AreEqual(excepted, actual, $"Provider created {actual} singleton objects");
    }
    
    [Test]
    public void CreateTransient()
    {
        InputData.Counter.count = 0;
        _configuration.Register<InputData.Counter, InputData.Counter>();

        _provider = new DependencyProvider(_configuration);
        
        _provider.Resolve<InputData.Counter>();
        _provider.Resolve<InputData.Counter>();
        _provider.Resolve<InputData.Counter>();
        var actual = _provider.Resolve<InputData.Counter>().Count;
        var excepted = 4;
        
        Assert.AreEqual(excepted, actual, $"Provider created {actual} transient objects");
    }
    
    [Test]
    public void CreateGenericImplementation()
    {
        _configuration.Register(typeof(IList<>), typeof(List<>));

        _provider = new DependencyProvider(_configuration);

        var actual = _provider.Resolve<IList<int>>();

        Assert.IsNotNull(_provider, "Incorrect work with generic types");
    }
    
    [Test]
    public void UseId()
    {
        _configuration.Register<InputData.IService, InputData.Service1>(InputData.ServiceImplementations.First);
        _configuration.Register<InputData.IService, InputData.Service2>(InputData.ServiceImplementations.Second);

        _provider = new DependencyProvider(_configuration);

        var actual = _provider.Resolve<InputData.IService>(InputData.ServiceImplementations.Second).GetName();

        var excepted = "Service2";
        Assert.AreEqual(excepted, actual, "Incorrect work with id");
    }

}