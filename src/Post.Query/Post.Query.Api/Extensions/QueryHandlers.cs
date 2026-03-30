using CQRS.Core.Infrastructure;
using Post.Query.Api.Queries;
using Post.Query.Domain.Entities;
using Post.Query.Infrastructure.Dispatchers;

namespace Post.Query.Api.Extensions;

public static class QueryHandlers
{
    public static void RegisterQueryHandlers(this IServiceCollection services)
    {
        var queryHandler = services.BuildServiceProvider().GetService<IQueryHandler>();
        var dispatcher = new QueryDispatcher();
        dispatcher.RegisterHandler<FindAllPostsQuery>(query => queryHandler!.HandleAsync(query));
        dispatcher.RegisterHandler<FindPostByIdQuery>(queryHandler!.HandleAsync);
        dispatcher.RegisterHandler<FindPostsByAuthorQuery>(queryHandler!.HandleAsync);
        dispatcher.RegisterHandler<FindPostsWithCommentsQuery>(queryHandler!.HandleAsync);
        dispatcher.RegisterHandler<FindPostWithLikesQuery>(queryHandler!.HandleAsync);

        // Register the dispatcher as a singleton service to only register the handlers once and reuse the same instance throughout the application
        services.AddSingleton<IQueryDispatcher<PostEntity>>(_ => dispatcher);
    }
}
