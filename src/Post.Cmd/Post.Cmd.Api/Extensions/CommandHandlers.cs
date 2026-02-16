using CQRS.Core.Infrastructure;
using Post.Cmd.Api.Commands;
using Post.Cmd.Infrastructure.Dispatchers;

namespace Post.Cmd.Api.Extensions
{
    public static class CommandHandlers
    {
        public static void RegisterCommandHandlers(this IServiceCollection services)
        {
            var commandHandler = services.BuildServiceProvider().GetService<ICommandHandler>();
            var dispatcher = new CommandDispatcher();
            dispatcher.RegisterHandler<NewPostCommand>(commandHandler!.HandleAsync);
            dispatcher.RegisterHandler<AddCommentCommand>(commandHandler!.HandleAsync);
            dispatcher.RegisterHandler<DeletePostCommand>(commandHandler!.HandleAsync);
            dispatcher.RegisterHandler<EditCommentCommand>(commandHandler!.HandleAsync);
            dispatcher.RegisterHandler<EditMessageCommand>(commandHandler!.HandleAsync);
            dispatcher.RegisterHandler<LikePostCommand>(commandHandler!.HandleAsync);
            dispatcher.RegisterHandler<RemoveCommentCommand>(commandHandler!.HandleAsync);

            services.AddSingleton<ICommandDispatcher>(_ => dispatcher);
        }
    }
}
