using api.Models;
using api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JSON serialization for formatted output
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Register services
builder.Services.AddHttpClient<GoogleMapsService>();
builder.Services.AddScoped<RouteOptimizationService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use CORS
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

// Root GET for testing purposes
app.MapGet("/", () => "API is running");

app.MapPost("/api/delivery-routes", async (DeliveryRouteRequest request, GoogleMapsService mapsService, RouteOptimizationService optimizationService) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    try
    {
        if (request.Origins == null || request.Origins.Count == 0)
        {
            return Results.Problem(
                detail: "At least one origin is required",
                statusCode: 400,
                title: "Validation Error"
            );
        }
        
        if (request.Destinations == null || request.Destinations.Count == 0)
        {
            return Results.Problem(
                detail: "At least one destination is required",
                statusCode: 400,
                title: "Validation Error"
            );
        }
        
        // Validate that each location has coordinates
        foreach (var origin in request.Origins)
        {
            if (!origin.Lat.HasValue || !origin.Lng.HasValue)
            {
                return Results.Problem(
                    detail: "Each origin must have latitude and longitude coordinates",
                    statusCode: 400,
                    title: "Validation Error"
                );
            }
        }
        
        foreach (var destination in request.Destinations)
        {
            if (!destination.Lat.HasValue || !destination.Lng.HasValue)
            {
                return Results.Problem(
                    detail: "Each destination must have latitude and longitude coordinates",
                    statusCode: 400,
                    title: "Validation Error"
                );
            }
        }

        // First, get all possible routes from computeRouteMatrix API
        var routeMatrix = await mapsService.CalculateRoutesAsync(request);
        
        // Then, optimize the routes using the selected algorithm
        var optimizedResult = optimizationService.OptimizeRoutes(
            routeMatrix.Routes,
            request.Origins.Count,
            request.Destinations.Count,
            request.Algorithm
        );
        
        stopwatch.Stop();
        optimizedResult.CalculationTimeMs = stopwatch.Elapsed.TotalMilliseconds;
        
        return Results.Ok(optimizedResult);
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 502,
            title: "External API Error"
        );
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Internal Server Error"
        );
    }
})
.WithName("CalculateDeliveryRoutes")
.WithOpenApi();

app.Run();
