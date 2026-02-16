using CQRS.Core.Events;

namespace Post.Common.Events
{
    public class CommentAddedEvent() : BaseEvent(nameof(CommentAddedEvent))
    {
        public Guid CommentId { get; set; }
        public string Comment { get; set; } = default!;
        public string Username { get; set; } = default!;
        public DateTime CommentDate { get; set; }
    }
}
