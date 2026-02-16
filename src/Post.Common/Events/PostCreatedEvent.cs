using CQRS.Core.Events;

namespace Post.Common.Events
{
    public class PostCreatedEvent() : BaseEvent(nameof(PostCreatedEvent))
    {
        public string Author { get; set; } = default!;
        public string Message { get; set; } = default!;
        public DateTime DatePosted { get; set; }
    }
}
