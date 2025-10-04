using Microsoft.OpenApi.Models;
using Rozklady.Data;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev",
        policy =>
        {
            policy.WithOrigins("https://localhost:44474") // adres Twojego React dev server
                  .AllowAnyHeader()
                  .AllowAnyMethod();
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
//builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

builder.Services.AddScoped<ScraperService>();
builder.Services.AddScoped<GtfsGenerator>();
builder.Services.AddScoped<GtfsFacade>();

builder.Services.AddHostedService<GtfsDailyService>();

var app = builder.Build();

// Użyj CORS
app.UseCors("AllowReactDev");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
