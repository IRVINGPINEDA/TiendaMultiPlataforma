using Microsoft.EntityFrameworkCore;
using ProductHub.Api.Options;
using ProductHub.Api.Services.Storage;
using ProductHub.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=producthub.db";
var dbProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    switch (dbProvider.ToLowerInvariant())
    {
        case "sqlserver":
            options.UseSqlServer(connectionString);
            break;
        case "postgresql":
        case "postgres":
        case "npgsql":
            options.UseNpgsql(connectionString);
            break;
        default:
            options.UseSqlite(connectionString);
            break;
    }
});
builder.Services.AddScoped<IProductImageStorageService, ProductImageStorageService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClients", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

var uploadPath = builder.Configuration["Storage:UploadPath"] ?? "uploads/products";
var uploadFolder = Path.Combine(app.Environment.ContentRootPath, "wwwroot", uploadPath.Replace('/', Path.DirectorySeparatorChar));
Directory.CreateDirectory(uploadFolder);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseCors("AllowClients");

app.MapControllers();

app.Run();
