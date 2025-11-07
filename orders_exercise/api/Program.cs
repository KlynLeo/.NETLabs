using api.Common.Mapping;
using api.Common.Resolvers;
using api.Data;
using api.Features.Orders.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Orders API",
        Version = "v1",
        Description = "API for managing orders"
    });
});
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMediatR(typeof(Program));
builder.Services.AddMemoryCache();  

builder.Services.AddTransient<CategoryDisplayResolver>();
builder.Services.AddTransient<PriceFormatterResolver>();
builder.Services.AddTransient<PublishedAgeResolver>();
builder.Services.AddTransient<AuthorInitialsResolver>();
builder.Services.AddTransient<AvailabilityStatusResolver>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();





var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// Minimal API endpoint
app.MapPost("/orders", async (CreateOrderProfileRequest request, IMediator mediator) =>
{
    var result = await mediator.Send(request);
    return Results.Created($"/orders/{result.Id}", result);
});

app.UseMiddleware<CorrelationMiddleware>();


app.Run();
