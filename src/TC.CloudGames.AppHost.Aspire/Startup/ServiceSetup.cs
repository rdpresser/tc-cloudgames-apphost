using TC.CloudGames.AppHost.Aspire.Extensions;

namespace TC.CloudGames.AppHost.Aspire.Startup
{
    public static class ServiceSetup
    {
        public static (IResourceBuilder<PostgresServerResource> postgres, IResourceBuilder<PostgresDatabaseResource> userDb,
            IResourceBuilder<PostgresDatabaseResource> gameDb, IResourceBuilder<PostgresDatabaseResource> paymentDb,
            IResourceBuilder<PostgresDatabaseResource> maintenanceDb) ConfigurePostgres(
            IDistributedApplicationBuilder builder, ParameterRegistry registry)
        {
            var usersDbName = builder.Configuration["Database:UsersDbName"];
            var gamesDbName = builder.Configuration["Database:GamesDbName"];
            var paymentsDbName = builder.Configuration["Database:PaymentsDbName"];
            var maintenanceDbName = builder.Configuration["Database:MaintenanceDbName"];

            if (!registry.Contains("postgres-user") || !registry.Contains("postgres-password"))
                throw new InvalidOperationException("Missing Postgres credentials in ParameterRegistry.");

            var postgres = builder.AddPostgres(builder.Configuration["Database:Host"]!)
            .WithImage("postgres:latest")
            .WithContainerName("TC-CloudGames-Db")
            .WithDataVolume("tccloudgames_postgres_data", isReadOnly: false)
            .WithPgAdmin(options => options
                .WithImage("dpage/pgadmin4:latest")
                .WithVolume("tccloudgames_pgadmin_data", "/var/lib/pgadmin")
                .WithContainerName("TC-CloudGames-PgAdmin4")
                ////.WithParameterEnv("PGADMIN_DEFAULT_EMAIL", registry["pgadmin-user"])
                ////.WithParameterEnv("PGADMIN_DEFAULT_PASSWORD", registry["pgadmin-password"])
                ////.WithEndpoint(port: 8080, targetPort: 80, name: "pgadmin-web")
                )
            .WithUserName(registry["postgres-user"].Resource)
            .WithPassword(registry["postgres-password"].Resource)
            .WithHostPort(65432);

            var userDb = postgres.AddDatabase("UsersDbConnection", usersDbName);
            var gamesDb = postgres.AddDatabase("GamesDbConnection", gamesDbName);
            var paymentsDb = postgres.AddDatabase("PaymentsDbConnection", paymentsDbName);
            var maintenanceDb = postgres.AddDatabase("MaintenanceDbConnection", maintenanceDbName);

            return (postgres, userDb, gamesDb, paymentsDb, maintenanceDb);
        }

        public static IResourceBuilder<RedisResource> ConfigureRedis(IDistributedApplicationBuilder builder, ParameterRegistry registry)
        {
            int redisPort = int.Parse(registry["redis-port"].Value!);
            return builder.AddRedis(
                    name: builder.Configuration["Cache:Host"]!,
                    port: redisPort,
                    password: registry["redis-password"].Resource)
                .WithImage("redis:latest")
                .WithContainerName("TC-CloudGames-Redis")
                .WithDataVolume("tccloudgames_redis_data", isReadOnly: false);
        }

        public static IResourceBuilder<RabbitMQServerResource> ConfigureRabbitMQ(IDistributedApplicationBuilder builder, ParameterRegistry registry)
        {
            return builder.AddRabbitMQ(builder.Configuration["RabbitMq:Host"]!, userName: registry["rabbitmq-user"].Resource, password: registry["rabbitmq-password"].Resource, port: 55672)
                .WithContainerName("TC-CloudGames-RabbitMq")
                .WithDataVolume("tccloudgames_rabbitmq_data", isReadOnly: false)
                .WithManagementPlugin(15672);
        }
    }
}