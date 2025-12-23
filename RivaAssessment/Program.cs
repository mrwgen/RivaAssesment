using RivaAssessment.BackgroundJobs;
using RivaAssessment.Infrastructure;
using RivaAssessment.Middleware;
using RivaAssessment.Models;
using RivaAssessment.Repositories;
using RivaAssessment.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the legacy billing repository
builder.Services.AddSingleton<ILegacyBillingRepository, LegacyBillingRepository>();
// Register the user repository
builder.Services.AddSingleton<IUserRepository, UserRepository>();

// Configure options from appsettings.json
builder.Services.Configure<CreditPersistenceOptions>(
    builder.Configuration.GetSection("CreditPersistence"));
builder.Services.Configure<CreditRefillOptions>(
    builder.Configuration.GetSection("CreditRefill"));
builder.Services.Configure<AuthenticationOptions>(
    builder.Configuration.GetSection("Authentication"));
//  Register the retry policy
builder.Services.AddSingleton<IRetryPolicy, RetryPolicy>();
// Register the credit service (you'll implement this)
builder.Services.AddSingleton<ICreditService, CreditService>();
builder.Services.AddSingleton<ICreditRefillService, CreditRefillService>();
builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();

//Register the credit refill background service and its dependencies
builder.Services.AddHostedService<CreditRefillBackgroundService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Riva Assessment API v1");
        c.RoutePrefix = "swagger"; // Swagger UI will be available at /swagger
    });
}

// HTTPS redirection - disabled for development to allow Swagger on HTTP
// Uncomment the line below if you set up HTTPS certificate: dotnet dev-certs https --trust
// app.UseHttpsRedirection();

// Register the Credit Enforcement Middleware

app.UseMiddleware<CreditEnforcementMiddleware>();
// End of TODO
app.MapControllers();

// Authorization - commented out since no authentication is configured
// Uncomment if you add authentication later
app.UseAuthorization();

app.MapControllers();

app.Run();

