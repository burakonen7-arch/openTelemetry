using System.Diagnostics;
using System.Diagnostics.Metrics;
namespace Product.Application.Products;

public static class ProductTelemetry
{
    public const string Name = "Product.Catalog";
    public static readonly ActivitySource Activities = new(Name);
    public static readonly Meter Meter = new(Name);
    public static readonly Counter<long> Created = Meter.CreateCounter<long>("products.created", "{product}");
}
