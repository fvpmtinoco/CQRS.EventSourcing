namespace Post.Cmd.Infrastructure.Config
{
    public record MongoDbConfig
    {
        public required string ConnectionString { get; set; }
        public required string Database { get; set; } = default!;
        public required string Collection { get; set; }
    }
}
