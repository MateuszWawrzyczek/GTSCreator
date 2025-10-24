using Microsoft.OpenApi.Models;
using Rozklady.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy
                .WithOrigins(
                    "https://localhost:44474", 
                    "http://localhost:44474"   
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.WithOrigins("http://149.202.38.111")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

builder.Services.AddDbContextFactory<RozkladyContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<KiedyPrzyjedzieClient>(client =>
{
    client.BaseAddress = new Uri("https://kiedyprzyjedzie.pl");
});
builder.Services.AddHttpClient<IKiedyPrzyjedzieClient, KiedyPrzyjedzieClient>(client =>
{
    client.BaseAddress = new Uri("https://kiedyprzyjedzie.pl");
});

builder.Services.AddHttpClient();
builder.Services.AddSingleton<TripsHistoryService>();
builder.Services.AddHostedService<RealTimeVehiclesService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

builder.Services.AddScoped<ScraperService>();
builder.Services.AddScoped<GtfsGenerator>();
builder.Services.AddScoped<GtfsFacade>();
builder.Services.AddScoped<GtfsUploader>();
builder.Services.AddHostedService<GtfsBackgroundService>();
builder.Services.AddSingleton<PrefixService>();
builder.Services.AddHostedService<PrefixUpdateService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.Urls.Add("http://localhost:7002");
}
else
{
    app.Urls.Add("http://0.0.0.0:5000");
}

app.UseCors("AllowReact");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

var prefixService = app.Services.GetRequiredService<PrefixService>();
await prefixService.RefreshPrefixesAsync(); 

app.Run();
