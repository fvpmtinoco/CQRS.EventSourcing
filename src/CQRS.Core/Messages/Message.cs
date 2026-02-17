using System.Text.Json.Serialization;

namespace CQRS.Core.Messages
{
    public abstract class Message
    {
        [JsonIgnore]
        public Guid Id { get; set; }
    }
}
