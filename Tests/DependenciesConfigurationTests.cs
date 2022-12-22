using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DependencyInjectionContainer;
using NUnit.Framework;

namespace Tests;

public class DependenciesConfigurationTests
{
    private DependenciesConfiguration _configuration = null!;

    [SetUp]
    public void Setup()
    {
        _configuration = new DependenciesConfiguration();
    }

    [Test]
    public void GetRealizedDependency()
    {
        _configuration.Register<IEnumerable<int>, List<int>>();
        _configuration.Register<IEnumerable<int>, ConcurrentBag<int>>();

        var actual = _configuration.GetImplementationsDescriptions<IEnumerable<int>>()
            .Select(des => des.ToType()).ToArray();

        var isContainsAll = actual.Contains(typeof(List<int>))
                            && actual.Contains(typeof(ConcurrentBag<int>));

        Assert.IsTrue(isContainsAll, $"GetImplementations returns invalid value");
    }

    [Test]
    public void GetNotRealizedDependency()
    {
        try
        {
            _configuration.GetImplementationsDescriptions<int>();
        }
        catch (DependenciesConfigurationException e)
        {
            Assert.Pass();
        }

        Assert.Fail("GetImplementation<int>() doesn't return DependenciesConfigurationException");
    }

    [Test]
    public void GetDependencyWithId()
    {
        _configuration.Register<InputData.IService, InputData.Service1>(InputData.ServiceImplementations.First);
        _configuration.Register<InputData.IService, InputData.Service2>(InputData.ServiceImplementations.Second);

        var actual =
            _configuration.GetImplementationDescription<InputData.IService>(InputData.ServiceImplementations.Second).ToType();

        var excepted = typeof(InputData.Service2);

        Assert.AreEqual(excepted, actual, "GetImplementation returns invalid value");
    }
    
    [Test]
    public void GetDependencyWithNotExistedId()
    {
        _configuration.Register<InputData.IService, InputData.Service1>(InputData.ServiceImplementations.First);

        try
        {
            _configuration.GetImplementationDescription<InputData.IService>(InputData.ServiceImplementations.Second);
        }
        catch (DependenciesConfigurationException)
        {
            Assert.Pass();
        }
        
        Assert.Fail("GetImplementation returns invalid value");
    }
    
    [Test]
    public void GetAllDependencies()
    {
        _configuration.Register<InputData.IService, InputData.Service1>(InputData.ServiceImplementations.First);
        _configuration.Register<InputData.IService, InputData.Service2>(InputData.ServiceImplementations.Second);
        _configuration.Register<InputData.AbstractService, InputData.Service1>(InputData.ServiceImplementations.First);
        _configuration.Register<InputData.IRepository, InputData.RepositoryImpl>();

        var actual = _configuration.GetAllDependencies();

        var isCorrect = actual.Contains(typeof(InputData.IService))
                        && actual.Contains(typeof(InputData.AbstractService))
                        && actual.Contains(typeof(InputData.IRepository));
        
        Assert.IsTrue(isCorrect, "GetAllDependencies returns invalid value");
    }
}