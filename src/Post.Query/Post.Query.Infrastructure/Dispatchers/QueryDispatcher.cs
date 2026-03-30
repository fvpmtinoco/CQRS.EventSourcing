using CQRS.Core.Infrastructure;
using CQRS.Core.Queries;
using Post.Query.Domain.Entities;

namespace Post.Query.Infrastructure.Dispatchers;

public class QueryDispatcher : IQueryDispatcher<PostEntity>
{
    private readonly Dictionary<Type, Func<BaseQuery, Task<List<PostEntity>>>> _handlers = [];

    public void RegisterHandler<TQuery>(Func<TQuery, Task<List<PostEntity>>> handler) where TQuery : BaseQuery
    {
        if (_handlers.ContainsKey(typeof(TQuery)))
        {
            throw new InvalidOperationException($"Handler for query type {typeof(TQuery).Name} is already registered.");
        }

        _handlers[typeof(TQuery)] = async (query) => await handler((TQuery)query);
    }

    public Task<List<PostEntity>> SendAsync(BaseQuery query)
    {
        if (_handlers.TryGetValue(query.GetType(), out var handler))
        {
            return handler(query);
        }
        else
        {
            throw new InvalidOperationException($"No handler registered for query type {query.GetType().Name}.");
        }
    }
}
