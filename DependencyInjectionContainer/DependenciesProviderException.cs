namespace DependencyInjectionContainer;

public class DependenciesProviderException : Exception
{
    public DependenciesProviderException() { }

    public DependenciesProviderException(string message)
        : base(message) { }

    public DependenciesProviderException(string message, Exception inner)
        : base(message, inner) { }
}