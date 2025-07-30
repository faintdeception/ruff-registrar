using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Read Keycloak configuration from appsettings.json
var keycloakConfig = builder.Configuration.GetSection("Keycloak");
var keycloakRealm = keycloakConfig["Realm"] ?? "student-registrar";
var keycloakClientId = keycloakConfig["ClientId"] ?? "student-registrar";
var keycloakClientSecret = keycloakConfig["ClientSecret"] ?? throw new InvalidOperationException("Keycloak ClientSecret is required in appsettings.json");

// PostgreSQL database - let Aspire generate password automatically
var postgres = builder.AddPostgres("postgres");

var studentRegistrarDb = postgres.AddDatabase("studentregistrar");

// Keycloak for authentication - use explicit password for consistency with persistent data
// Add persistent data volume so realm/roles configuration persists
var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithDataVolume()
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin123");

// API service
var apiService = builder.AddProject<StudentRegistrar_Api>("api")
    .WithReference(studentRegistrarDb)
    .WithReference(keycloak)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("Keycloak__Realm", keycloakRealm)
    .WithEnvironment("Keycloak__ClientId", keycloakClientId)
    .WithEnvironment("Keycloak__ClientSecret", keycloakClientSecret);

// Next.js frontend
var frontend = builder.AddNpmApp("frontend", "../../frontend", "dev")
    .WithReference(apiService)
    .WithHttpEndpoint(port: 3001, env: "PORT")
    .WithEnvironment("NODE_ENV", "development")
    .WithEnvironment("NEXT_TELEMETRY_DISABLED", "1")
    .WithExternalHttpEndpoints();

// Configure the frontend to use the API
frontend.WithEnvironment("NEXT_PUBLIC_API_URL", apiService.GetEndpoint("http"));

await builder.Build().RunAsync();
