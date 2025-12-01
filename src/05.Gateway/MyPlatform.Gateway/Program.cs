using MyPlatform.SDK.Authentication.Extensions;
using MyPlatform.SDK.Observability.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddPlatformAuthentication(builder.Configuration);
builder.Services.AddPlatformObservability(builder.Configuration);

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

app.Run();
