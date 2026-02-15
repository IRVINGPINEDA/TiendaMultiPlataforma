using Microsoft.Extensions.Options;
using ProductHub.Web.Options;
using ProductHub.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));
builder.Services.AddHttpClient<IProductsApiClient, ProductsApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;
    var baseUrl = string.IsNullOrWhiteSpace(options.BaseUrl)
        ? "https://localhost:7219/"
        : options.BaseUrl;

    if (!baseUrl.EndsWith('/'))
    {
        baseUrl += "/";
    }

    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
});

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
