using TC.CloudGames.AppHost.Aspire.Extensions;

namespace TC.CloudGames.AppHost.Aspire.Startup
{
    public static class ProjectSetup
    {
        public static void ConfigureUsersApi(
            IDistributedApplicationBuilder builder,
            ParameterRegistry registry,
            IResourceBuilder<PostgresServerResource> postgres,
            IResourceBuilder<PostgresDatabaseResource> userDb,
            IResourceBuilder<PostgresDatabaseResource> maintenanceDb,
            IResourceBuilder<RedisResource> redis,
            IResourceBuilder<RabbitMQServerResource> rabbit)
        {
            builder.AddProject<Projects.TC_CloudGames_Users_Api>("users-api")
                // Health checks via HTTP endpoints
                .WithHttpHealthCheck("/health")
                .WithHttpHealthCheck("/ready")
                .WithHttpHealthCheck("/live")
                .WithReference(postgres)   // Needed for schema/table creation via Marten/Wolverine
                .WithReference(userDb)     // Needed for DB connection
                .WithReference(maintenanceDb)
                .WithReference(redis)
                .WithReference(rabbit)

                // PostgreSQL
                .WithEnvironment("DB_HOST", "localhost")
                .WithEnvironment("DB_PORT", builder.Configuration["Database:Port"])
                .WithEnvironment("DB_NAME", userDb.Resource.DatabaseName)
                .WithEnvironment("DB_MAINTENANCE_NAME", maintenanceDb.Resource.DatabaseName)
                .WithParameterEnv("DB_USER", registry["postgres-user"])
                .WithParameterEnv("DB_PASSWORD", registry["postgres-password"])

            // RabbitMQ
            .WithEnvironment("RABBITMQ_HOST", "localhost")
            .WithEnvironment("RABBITMQ_PORT", builder.Configuration["RabbitMq:Port"])
            .WithEnvironment("RABBITMQ_VHOST", builder.Configuration["RabbitMq:VirtualHost"])
            .WithEnvironment("RABBITMQ_EXCHANGE", builder.Configuration["RabbitMq:Exchange"])
            .WithParameterEnv("RABBITMQ_USERNAME", registry["rabbitmq-user"])
            .WithParameterEnv("RABBITMQ_PASSWORD", registry["rabbitmq-password"])

            // Redis
            .WithEnvironment("CACHE_HOST", "localhost")
            .WithEnvironment("CACHE_PORT", builder.Configuration["Cache:Port"])
            .WithParameterEnv("CACHE_PASSWORD", registry["redis-password"])
            .WithEnvironment("CACHE_SECURE", "false");

            ////// ASP.NET Core
            ////.WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");
        }
    }
}
