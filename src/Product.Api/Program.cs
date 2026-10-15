using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Product.Application.Products;
using Product.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<ProductDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("Products")
    ?? throw new InvalidOperationException("ConnectionStrings__Products is required.")));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("product-api"))
    .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation()
        .AddNpgsql().AddSource(ProductTelemetry.Name).AddOtlpExporter())
    .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation().AddMeter(ProductTelemetry.Name).AddOtlpExporter());
builder.Logging.AddOpenTelemetry(logging => {
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.AddOtlpExporter();
});
var app = builder.Build();
app.UseExceptionHandler();
app.MapControllers();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
if (args.Contains("--migrate"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<ProductDbContext>().Database.MigrateAsync();
    return;
}
await app.RunAsync();
public partial class Program;
