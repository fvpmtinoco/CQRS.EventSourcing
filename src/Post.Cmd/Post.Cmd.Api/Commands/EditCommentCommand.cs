using CQRS.Core.Commands;

namespace Post.Cmd.Api.Commands
{
    public class EditCommentCommand : BaseCommand
    {
        public Guid CommendId { get; set; }
        public string Comment { get; set; } = default!;
        public string Username { get; set; } = default!;
    }
}
