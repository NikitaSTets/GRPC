using DiscountService;
using DiscountService.Interfaces;
using DiscountService.Services;
using Storage;
using Storage.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddScoped<ICodeValidationService, CodeValidationService>();
builder.Services.AddScoped<ICodeStorageService>(provider =>
{
    return new FileCodeStorageService("discount_codes.json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<DiscountRPCService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
