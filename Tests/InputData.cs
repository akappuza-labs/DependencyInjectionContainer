using System.Threading;
using NuGet.Frameworks;

namespace Tests;

public class InputData
{
    public enum ServiceImplementations : int
    {
        First,
        Second
    }

    public interface IService
    {
        public string GetName();
    }
    public abstract class AbstractService { }
    
    
    public class Service1 : AbstractService, IService
    {
        public string GetName()
        {
            return "Service1";
        }
    }
    public class Service2 : AbstractService, IService
    {
        public string GetName()
        {
            return "Service2";
        }
    }

    public class ServiceImpl : IService
    {
        private IRepository _repository;

        public ServiceImpl(IRepository repository)
        {
            _repository = repository;
        }
        public string GetName()
        {
            return $"ServiceImpl <- {_repository.GetName()}";
        }
    }

    public interface IRepository
    {
        public string GetName();
    }

    public class RepositoryImpl : IRepository
    {
        public RepositoryImpl() {}

        public string GetName()
        {
            return "RepositoryImpl";
        }
    }
    
    public class Counter
    {
        public static int count = 0;

        public int Count
        {
            get => count;
            set => count = value;
        }
        public Counter()
        {
            Interlocked.Increment(ref count);
        }
    }

}