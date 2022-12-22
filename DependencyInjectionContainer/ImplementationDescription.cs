namespace DependencyInjectionContainer;

public struct ImplementationDescription
{
    public Enum? Id;
    public Type Type;
    public Lifecycle Lifecycle;

    public ImplementationDescription(Enum? id, Type type, Lifecycle lifecycle)
    {
        Id = id;
        Type = type;
        Lifecycle = lifecycle;
    }

    public Type ToType()
    {
        return Type;
    }
}