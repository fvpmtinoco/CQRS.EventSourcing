namespace CQRS.Core.Exceptions
{
    public class AggregateNotFoundException(string message) : Exception(message)
    {

    }
}
