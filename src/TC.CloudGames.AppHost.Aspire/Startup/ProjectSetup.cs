using TC.CloudGames.AppHost.Aspire.Extensions;

namespace TC.CloudGames.AppHost.Aspire.Startup
{
    public static class ProjectSetup
    {
        public static void ConfigureUsersApi(
            IDistributedApplicationBuilder builder,
            ParameterRegistry registry,
            IResourceBuilder<PostgresServerResource> postgres,
            IResourceBuilder<PostgresServerResource> userDb,
            IResourceBuilder<RedisResource> redis,
            IResourceBuilder<RabbitMQServerResource> rabbit)
        {
            builder.AddProject<Projects.TC_CloudGames_Users_Api>("users-api")
                .WithReference(postgres)   // Needed for schema/table creation via Marten/Wolverine
                .WithReference(userDb)     // Needed for DB connection
                .WithReference(redis)
                .WithReference(rabbit)

                // PostgreSQL
                .WithEnvironment("DB_HOST", builder.Configuration["Database:Host"])
                .WithEnvironment("DB_PORT", builder.Configuration["Database:Port"])
                .WithEnvironment("DB_NAME", builder.Configuration["Database:Name"])
                .WithParameterEnv("DB_USER", registry["postgres-user"])
                .WithParameterEnv("DB_PASSWORD", registry["postgres-password"])

                // RabbitMQ
                .WithEnvironment("RABBITMQ_HOST", builder.Configuration["RabbitMq:Host"])
                .WithEnvironment("RABBITMQ_PORT", builder.Configuration["RabbitMq:Port"])
                .WithEnvironment("RABBITMQ_VHOST", builder.Configuration["RabbitMq:VirtualHost"])
                .WithEnvironment("RABBITMQ_EXCHANGE", builder.Configuration["RabbitMq:Exchange"])
                .WithParameterEnv("RABBITMQ_USERNAME", registry["rabbitmq-user"])
                .WithParameterEnv("RABBITMQ_PASSWORD", registry["rabbitmq-password"])

                // Redis
                .WithEnvironment("CACHE_HOST", builder.Configuration["Cache:Host"])
                .WithEnvironment("CACHE_PORT", builder.Configuration["Cache:Port"])
                .WithParameterEnv("CACHE_PASSWORD", registry["redis-password"])

                // ASP.NET Core
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");
        }
    }
}
