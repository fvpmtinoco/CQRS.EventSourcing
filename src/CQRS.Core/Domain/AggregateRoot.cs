using CQRS.Core.Events;
using System.Reflection;

namespace CQRS.Core.Domain
{
    public abstract class AggregateRoot
    {
        protected Guid _id;
        private readonly List<BaseEvent> _changes = [];

        public Guid Id => _id;

        public int Version { get; set; } = default;

        public IEnumerable<BaseEvent> GetUncommitedChanges() => _changes;

        public void MarkChangesAsCommited() => _changes.Clear();

        private void ApplyChanges(BaseEvent @event, bool isNew)
        {
            MethodInfo? method = this.GetType().GetMethod("Apply", [@event.GetType()]);

            if (method is null)
            {
                throw new ArgumentNullException(nameof(method), $"The Apply method was not found in the aggregate for {@event.GetType().Name}.");
            }

            method.Invoke(this, [@event]);

            if (isNew)
            {
                _changes.Add(@event);
            }
        }

        protected void RaiseEvent(BaseEvent @event)
        {
            ApplyChanges(@event, true);
        }

        public void ReplayEvents(IEnumerable<BaseEvent> events)
        {
            foreach (BaseEvent e in events)
            {
                ApplyChanges(e, false);
            }
        }
    }
}
