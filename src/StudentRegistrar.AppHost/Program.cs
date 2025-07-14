using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL database - let Aspire generate password automatically
var postgres = builder.AddPostgres("postgres");

var studentRegistrarDb = postgres.AddDatabase("studentregistrar");

// Keycloak for authentication - let Aspire generate password automatically
var keycloak = builder.AddKeycloak("keycloak", 8080);

// API service
var apiService = builder.AddProject<StudentRegistrar_Api>("api")
    .WithReference(studentRegistrarDb)
    .WithReference(keycloak)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithHttpEndpoint(port: 5000, name: "http");

// Next.js frontend
var frontend = builder.AddNpmApp("frontend", "../../frontend", "dev")
    .WithReference(apiService)
    .WithHttpEndpoint(port: 3001, env: "PORT")
    .WithEnvironment("NODE_ENV", "development")
    .WithEnvironment("NEXT_TELEMETRY_DISABLED", "1")
    .WithExternalHttpEndpoints();

// Configure the frontend to use the API
frontend.WithEnvironment("NEXT_PUBLIC_API_URL", "http://localhost:5000");

await builder.Build().RunAsync();
