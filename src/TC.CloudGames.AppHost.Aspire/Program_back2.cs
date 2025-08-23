////using Microsoft.Extensions.Logging;
////using TC.CloudGames.AppHost.Aspire.Extensions;

////var builder = DistributedApplication.CreateBuilder(args);

//////.env vars
////DotNetEnv.Env.Load(Path.Combine("./", ".env"));

////// Logger Factory
////var loggerFactory = LoggerFactory.Create(config =>
////{
////    config.AddConsole();
////    config.SetMinimumLevel(LogLevel.Information);
////});
////var logger = loggerFactory.CreateLogger("Startup");

////// Criando os parâmetros
////var registry = new ParameterRegistry();
////var postgresUser = registry.Add(builder, "postgres-user", "Database:User", "DB_USER", "postgres");
////var postgresPassword = registry.Add(builder, "postgres-password", "Database:Password", "DB_PASSWORD", "postgres", secret: true);
////var rabbitUser = registry.Add(builder, "rabbitmq-user", "RabbitMq:UserName", "RABBITMQ_USERNAME", "guest");
////var rabbitPassword = registry.Add(builder, "rabbitmq-password", "RabbitMq:Password", "RABBITMQ_PASSWORD", "guest", secret: true);
////var redisPassword = registry.Add(builder, "redis-password", "Cache:Password", "CACHE_PASSWORD", "", secret: true);

////// Log no startup
////registry.LogAll(builder.Configuration, logger);


////// -------------------- PostgreSQL --------------------
////var postgres = builder.AddPostgres("TC-CloudGames-DbPg-Host")
////    .WithImage("postgres:latest")
////    .WithUserName(postgresUser)
////    .WithPassword(postgresPassword)
////    .WithEnvironment("POSTGRES_DB", postgresDbName)
////    .WithBindMount("tccloudgames_pgdata", "/var/lib/postgresql/data")
////    .WithEndpoint(postgresDbPort, 54325);

////var userDb = postgres.AddDatabase(postgresDbName);

////// -------------------- Redis --------------------
////var redis = builder.AddRedis("TC-CloudGames-Redis-Host")
////    .WithImage("redis:latest")
////    .WithBindMount("tccloudgames_redisdata", "/data")
////    .WithArgs("redis-server", "--appendonly", "yes", "--appendfilename", "appendonly.aof", "--dir", "/data")
////    .WithEndpoint(6379, 6379)
////    .WithParameterEnv("REDIS_PASSWORD", redisPassword);

////// -------------------- RabbitMQ --------------------
////var rabbit = builder.AddRabbitMQ("TC-CloudGames-RabbitMq-Host")
////    .WithImage("rabbitmq:3-management")
////    .WithParameterEnv("RABBITMQ_DEFAULT_USER", rabbitUser)
////    .WithParameterEnv("RABBITMQ_DEFAULT_PASS", rabbitPassword)
////    .WithEnvironment("RABBITMQ_DEFAULT_VHOST", "/")
////    .WithBindMount("tccloudgames_rabbitmq_data", "/var/lib/rabbitmq")
////    .WithEndpoint(5672, 5672, name: "amqp")
////    .WithEndpoint(15672, 15672, name: "management")
////    .WithEndpoint(15692, 15692, name: "metrics");

////// -------------------- API de Users --------------------

////// Aplicando no projeto Aspire
////builder.AddProject<Projects.TC_CloudGames_Users_Api>("users-api")
////    .WithReference(postgres)
////    .WithReference(redis)
////    .WithReference(rabbit)
////    // Banco de dados
////    .WithEnvironment("DB_HOST", builder.Configuration["Database:Host"])
////    .WithEnvironment("DB_PORT", builder.Configuration["Database:Port"])
////    .WithEnvironment("DB_NAME", builder.Configuration["Database:Name"])
////    .WithParameterEnv("DB_USER", postgresUser)
////    .WithParameterEnv("DB_PASSWORD", postgresPassword)
////    // RabbitMQ
////    .WithEnvironment("RABBITMQ_HOST", builder.Configuration["RabbitMq:Host"])
////    .WithEnvironment("RABBITMQ_PORT", builder.Configuration["RabbitMq:Port"])
////    .WithEnvironment("RABBITMQ_VHOST", builder.Configuration["RabbitMq:VirtualHost"])
////    .WithEnvironment("RABBITMQ_EXCHANGE", builder.Configuration["RabbitMq:Exchange"])
////    .WithParameterEnv("RABBITMQ_USERNAME", rabbitUser)
////    .WithParameterEnv("RABBITMQ_PASSWORD", rabbitPassword)
////    .WithParameterEnv("CACHE_PASSWORD", redisPassword)
////    // Redis
////    .WithEnvironment("CACHE_HOST", builder.Configuration["Cache:Host"])
////    .WithEnvironment("CACHE_PORT", builder.Configuration["Cache:Port"])
////    .WithParameterEnv("CACHE_PASSWORD", redisPassword)
////    // Ambiente
////    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

////await builder.Build().RunAsync();