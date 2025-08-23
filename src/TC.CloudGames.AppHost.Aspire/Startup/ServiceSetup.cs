using TC.CloudGames.AppHost.Aspire.Extensions;

namespace TC.CloudGames.AppHost.Aspire.Startup
{
    public static class ServiceSetup
    {
        public static (IResourceBuilder<PostgresServerResource> postgres, IResourceBuilder<PostgresDatabaseResource> userDb) ConfigurePostgres(
            IDistributedApplicationBuilder builder, ParameterRegistry registry)
        {
            var dbName = builder.Configuration["Database:Name"] ?? "tc_cloudgames_users";
            var dbPort = int.TryParse(builder.Configuration["Database:Port"], out var port) ? port : 5432;

            var postgres = builder.AddPostgres("TC-CloudGames-DbPg-Host")
                .WithImage("postgres:latest")
                .WithUserName(registry["postgres-user"])
                .WithPassword(registry["postgres-password"])
                .WithEnvironment("POSTGRES_DB", dbName)
                .WithBindMount("tccloudgames_pg_data", "/var/lib/postgresql/data")
                .WithEndpoint(dbPort, 54325);

            var userDb = postgres.AddDatabase(dbName);
            return (postgres, userDb);
        }

        public static IResourceBuilder<RedisResource> ConfigureRedis(IDistributedApplicationBuilder builder, ParameterRegistry registry)
        {
            return builder.AddRedis("TC-CloudGames-Redis-Host")
                .WithImage("redis:latest")
                .WithBindMount("tccloudgames_redis_data", "/data")
                .WithArgs("redis-server", "--appendonly", "yes", "--appendfilename", "appendonly.aof", "--dir", "/data")
                .WithEndpoint(6379, 6379)
                .WithParameterEnv("REDIS_PASSWORD", registry["redis-password"]);
        }

        public static IResourceBuilder<RabbitMQServerResource> ConfigureRabbitMQ(IDistributedApplicationBuilder builder, ParameterRegistry registry)
        {
            return builder.AddRabbitMQ("TC-CloudGames-RabbitMq-Host")
                .WithImage("rabbitmq:3-management")
                .WithParameterEnv("RABBITMQ_DEFAULT_USER", registry["rabbitmq-user"])
                .WithParameterEnv("RABBITMQ_DEFAULT_PASS", registry["rabbitmq-password"])
                .WithEnvironment("RABBITMQ_DEFAULT_VHOST", "/")
                .WithBindMount("tccloudgames_rabbitmq_data", "/var/lib/rabbitmq")
                .WithEndpoint(5672, 5672, name: "amqp")
                .WithEndpoint(15672, 15672, name: "management")
                .WithEndpoint(15692, 15692, name: "metrics");
        }
    }
}