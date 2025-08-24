using TC.CloudGames.AppHost.Aspire.Extensions;

namespace TC.CloudGames.AppHost.Aspire.Startup
{
    public static class ServiceSetup
    {
        public static (IResourceBuilder<PostgresServerResource> postgres, IResourceBuilder<PostgresDatabaseResource> userDb) ConfigurePostgres(
            IDistributedApplicationBuilder builder, ParameterRegistry registry)
        {
            var dbName = builder.Configuration["Database:Name"] ?? "tc-cloudgames-users-db";
            var dbPort = int.TryParse(builder.Configuration["Database:Port"], out var port) ? port : 5432;

            if (!registry.Contains("postgres-user") || !registry.Contains("postgres-password"))
                throw new InvalidOperationException("Missing Postgres credentials in ParameterRegistry.");

            var postgres = builder.AddPostgres("TC-CloudGames-DbPg-Host")
                .WithImage("postgres:latest")
                .WithHealthCheck("postgres-health")
                .WithUserName(registry["postgres-user"].Resource)
                .WithPassword(registry["postgres-password"].Resource)
                .WithEnvironment("POSTGRES_DB", dbName)
                .WithVolume("tccloudgames_pg_data", "/var/lib/postgresql/data")
                .WithEndpoint(dbPort, 54320, name: "postgres-db");

            var userDb = postgres.AddDatabase(dbName);
            return (postgres, userDb);
        }

        public static IResourceBuilder<RedisResource> ConfigureRedis(IDistributedApplicationBuilder builder, ParameterRegistry registry)
        {
            return builder.AddRedis("TC-CloudGames-Redis-Host")
                .WithImage("redis:latest")
                .WithHealthCheck("redis-health")
                .WithVolume("tccloudgames_redis_data", "/data")
                .WithArgs("redis-server", "--appendonly", "yes", "--appendfilename", "appendonly.aof", "--dir", "/data")
                .WithParameterEnv("REDIS_PASSWORD", registry["redis-password"])
                .WithEndpoint(6379, 6379, name: "redis-tcp");
        }

        public static IResourceBuilder<RabbitMQServerResource> ConfigureRabbitMQ(IDistributedApplicationBuilder builder, ParameterRegistry registry)
        {
            return builder.AddRabbitMQ("TC-CloudGames-RabbitMq-Host")
                .WithImage("rabbitmq:3-management")
                .WithHealthCheck("rabbitmq-health")
                .WithParameterEnv("RABBITMQ_DEFAULT_USER", registry["rabbitmq-user"])
                .WithParameterEnv("RABBITMQ_DEFAULT_PASS", registry["rabbitmq-password"])
                .WithEnvironment("RABBITMQ_DEFAULT_VHOST", "/")
                .WithVolume("tccloudgames_rabbitmq_data", "/var/lib/rabbitmq")
                .WithEndpoint(5672, 5672, name: "rabbitmq-amqp")
                .WithEndpoint(15672, 15672, name: "rabbitmq-management")
                .WithEndpoint(15692, 15692, name: "rabbitmq-metrics");
        }
    }
}