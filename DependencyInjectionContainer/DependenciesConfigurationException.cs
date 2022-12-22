namespace DependencyInjectionContainer;

public class DependenciesConfigurationException : Exception
{
    public DependenciesConfigurationException() { }

    public DependenciesConfigurationException(string message)
        : base(message) { }

    public DependenciesConfigurationException(string message, Exception inner)
        : base(message, inner) { }
}