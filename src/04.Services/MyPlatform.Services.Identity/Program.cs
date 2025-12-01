using MyPlatform.SDK.Authentication.Extensions;
using MyPlatform.SDK.Authorization.Extensions;
using MyPlatform.SDK.Caching.Extensions;
using MyPlatform.SDK.Core.Extensions;
using MyPlatform.SDK.EventBus.Extensions;
using MyPlatform.SDK.MultiTenancy.Extensions;
using MyPlatform.SDK.Observability.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add platform services
builder.Services.AddPlatformCore(builder.Configuration);
builder.Services.AddPlatformAuthentication(builder.Configuration);
builder.Services.AddPlatformAuthorization();
builder.Services.AddPlatformMultiTenancy();
builder.Services.AddPlatformCaching(builder.Configuration);
builder.Services.AddPlatformEventBus(builder.Configuration);
builder.Services.AddPlatformObservability(builder.Configuration);

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMultiTenancy();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
