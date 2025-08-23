var builder = DistributedApplication.CreateBuilder(args);

// Configuração do container PostgreSQL

var postgres_user = builder.AddParameter("postgres", secret: false);
var postgres_password = builder.AddParameter("postgres", secret: true);
var postgres_host_name = "TC-CloudGames-DbPg-Host";
var postgres_db_name = "tc_cloudgames_user_db";
var postgres_db_port = 5432;

var postgres = builder.AddPostgres(postgres_host_name)
    .WithImage("postgres:latest") // Imagem oficial do PostgreSQL
    .WithPassword(postgres_password) // Senha do usuário
    .WithUserName(postgres_user)     // Usuário do banco
    .WithEnvironment("POSTGRES_USER", postgres_user)     // Usuário padrão
    .WithEnvironment("POSTGRES_PASSWORD", postgres_password) // Senha padrão 
    .WithEnvironment("POSTGRES_DB", postgres_db_name) // Banco de dados inicial
    .WithBindMount("tccloudgames_pgdata", "/var/lib/postgresql/data") // Volume persistente   
    .WithEndpoint(postgres_db_port, 54325); // Mapeamento de porta fixa

// Criação do banco de dados específico para usuários
var userDb = postgres.AddDatabase(postgres_host_name, postgres_db_name);

// Redis
var redis = builder.AddRedis("TC-CloudGames-Redis-Host")
    .WithImage("redis:latest")
    .WithBindMount("tccloudgames_redisdata", "/data") // volume persistente
    .WithArgs("redis-server", "--appendonly", "yes", "--appendfilename", "appendonly.aof", "--dir", "/data")
    .WithEndpoint(6379, 6379); // porta fixa

// RabbitMQ
var rabbit = builder.AddRabbitMQ("TC-CloudGames-RabbitMq-Host")
    .WithImage("rabbitmq:3-management")
    .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
    .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
    .WithEnvironment("RABBITMQ_DEFAULT_VHOST", "/")
    .WithBindMount("tccloudgames_rabbitmq_data", "/var/lib/rabbitmq") // volume persistente
    .WithEndpoint(5672, 5672, name: "amqp")   // AMQP
    .WithEndpoint(15672, 15672, name: "management") // Management UI
    .WithEndpoint(15692, 15692, name: "metrics"); // Prometheus metrics

// API de Users
var usersApi = builder.AddProject<Projects.TC_CloudGames_Users_Api>("users-api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbit)
    // Variáveis banco de dados
    .WithEnvironment("DB_HOST", postgres.Resource.Name) // Aspire resolve para o hostname do container
    .WithEnvironment("DB_PORT", postgres_db_port.ToString()) // Porta interna do container
    .WithEnvironment("DB_USER", postgres_user)
    .WithEnvironment("DB_PASSWORD", postgres_password)
    .WithEnvironment("DB_NAME", postgres.Resource.Databases[postgres.Resource.Name])
    //Variáveis Redis
    .WithEnvironment("CACHE_HOST", redis.Resource.Name)
    .WithEnvironment("CACHE_PORT", "6379")
    .WithEnvironment("CACHE_PASSWORD", "")
    // Variáveis RabbitMQ
    .WithEnvironment("RABBITMQ_HOST", rabbit.Resource.Name)
    .WithEnvironment("RABBITMQ_PORT", "5672")
    .WithEnvironment("RABBITMQ_USERNAME", "guest")
    .WithEnvironment("RABBITMQ_PASSWORD", "guest")
    .WithEnvironment("RABBITMQ_VHOST", "/")
    .WithEnvironment("RABBITMQ_EXCHANGE", "user.events")
    // Outras variáveis
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development"); // Define o ambiente como Development


builder.Build().Run();