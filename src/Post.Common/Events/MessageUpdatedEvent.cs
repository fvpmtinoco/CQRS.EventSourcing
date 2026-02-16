using CQRS.Core.Events;

namespace Post.Common.Events
{
    public class MessageUpdatedEvent() : BaseEvent(nameof(MessageUpdatedEvent))
    {
        public string Message { get; set; } = default!;
    }
}
