using System.Text;
using System.Text.Json;
using api.Models;

namespace api.Services;

/// <summary>
/// Provides functionality to interact with Google Maps APIs for route calculation and distance matrix operations.
/// </summary>
public class GoogleMapsService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GoogleMapsService> _logger;
    private const string RouteMatrixUrl = "https://routes.googleapis.com/distanceMatrix/v2:computeRouteMatrix";

    /// <summary>
    /// Initializes a new instance of the GoogleMapsService class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making API requests.</param>
    /// <param name="configuration">The configuration containing Google Maps API settings.</param>
    /// <param name="logger">The logger for recording service operations.</param>
    /// <exception cref="InvalidOperationException">Google Maps API key is not configured in the application settings.</exception>
    public GoogleMapsService(HttpClient httpClient, IConfiguration configuration, ILogger<GoogleMapsService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GoogleMaps:ApiKey"] ?? throw new InvalidOperationException("Google Maps API key not configured");
        _logger = logger;
    }

    /// <summary>
    /// Calculates routes between origins and destinations using Google Maps APIs and local calculations.
    /// </summary>
    /// <param name="request">The delivery route request containing origins, destinations, and preferences.</param>
    /// <returns>A delivery route response containing all calculated routes.</returns>
    /// <exception cref="HttpRequestException">An error occurred when calling the Google Maps API.</exception>
    /// <exception cref="InvalidOperationException">Failed to deserialize the Google Maps API response.</exception>
    public async Task<DeliveryRouteResponse> CalculateRoutesAsync(DeliveryRouteRequest request)
    {
        var allRoutes = new List<RouteInfo>();
        
        // 1. Fetch routes from origins to destinations
        var originToDestRoutes = await FetchRouteMatrixAsync(
            request.Origins, 
            request.Destinations,
            request);
        allRoutes.AddRange(originToDestRoutes);
        
        // 2. Calculate routes between destinations using lat/lng (for sequential routing)
        if (request.Destinations.Count > 1)
        {
            var destToDestRoutes = CalculateDestinationToDestinationRoutes(request.Destinations);
            allRoutes.AddRange(destToDestRoutes);
        }
        
        return new DeliveryRouteResponse
        {
            Routes = allRoutes
        };
    }

    /// <summary>
    /// Fetches route matrix data from Google Maps API for the specified origins and destinations.
    /// </summary>
    /// <param name="origins">The collection of origin addresses.</param>
    /// <param name="destinations">The collection of destination addresses.</param>
    /// <param name="originalRequest">The original request containing vehicle emission type and other parameters.</param>
    /// <returns>A list of route information between origins and destinations.</returns>
    /// <exception cref="HttpRequestException">Google Maps API returned an error status code.</exception>
    /// <exception cref="InvalidOperationException">Failed to deserialize Google Maps API response.</exception>
    private async Task<List<RouteInfo>> FetchRouteMatrixAsync(
        List<AddressLocation> origins,
        List<AddressLocation> destinations,
        DeliveryRouteRequest originalRequest)
    {
        var googleRequest = new RouteMatrixRequest
        {
            Origins = origins.Select(o => new RouteMatrixOrigin
            {
                Waypoint = new Waypoint 
                { 
                    PlaceId = o.PlaceId,
                    Address = string.IsNullOrWhiteSpace(o.PlaceId) ? o.Address : null
                },
                RouteModifiers = new RouteModifiers
                {
                    VehicleInfo = new VehicleInfo
                    {
                        EmissionType = originalRequest.VehicleEmissionType.ToString().ToUpperInvariant()
                    }
                }
            }).ToList(),
            Destinations = destinations.Select(d => new RouteMatrixDestination
            {
                Waypoint = new Waypoint 
                { 
                    PlaceId = d.PlaceId,
                    Address = string.IsNullOrWhiteSpace(d.PlaceId) ? d.Address : null
                }
            }).ToList()
        };

        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        var json = JsonSerializer.Serialize(googleRequest, options);
        _logger.LogInformation("Sending request to Google Maps API: {Json}", json);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, RouteMatrixUrl)
        {
            Content = content
        };
        requestMessage.Headers.Add("X-Goog-Api-Key", _apiKey);
        requestMessage.Headers.Add("X-Goog-FieldMask", "originIndex,destinationIndex,status,distanceMeters,duration,condition");

        try
        {
            var response = await _httpClient.SendAsync(requestMessage);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google Maps API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                throw new HttpRequestException($"Google Maps API returned status code {response.StatusCode}");
            }

            var apiResponse = JsonSerializer.Deserialize<List<RouteMatrixResponse>>(responseContent);
            
            if (apiResponse == null)
            {
                throw new InvalidOperationException("Failed to deserialize Google Maps API response");
            }

            var routes = apiResponse.Select(r => new RouteInfo
            {
                OriginIndex = r.OriginIndex,
                DestinationIndex = r.DestinationIndex,
                OriginAddress = origins[r.OriginIndex].Address ?? origins[r.OriginIndex].PlaceId ?? "Unknown",
                DestinationAddress = destinations[r.DestinationIndex].Address ?? destinations[r.DestinationIndex].PlaceId ?? "Unknown",
                DistanceMeters = r.DistanceMeters,
                Duration = r.Duration ?? "N/A",
                Status = r.Status?.Message ?? (r.Status?.Code == 0 ? "OK" : "ERROR"),
                OriginLocation = origins[r.OriginIndex].Lat.HasValue && origins[r.OriginIndex].Lng.HasValue
                    ? new LatLng { Lat = origins[r.OriginIndex].Lat!.Value, Lng = origins[r.OriginIndex].Lng!.Value }
                    : null,
                DestinationLocation = destinations[r.DestinationIndex].Lat.HasValue && destinations[r.DestinationIndex].Lng.HasValue
                    ? new LatLng { Lat = destinations[r.DestinationIndex].Lat!.Value, Lng = destinations[r.DestinationIndex].Lng!.Value }
                    : null
            }).ToList();

            return routes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google Maps API");
            throw;
        }
    }

    /// <summary>
    /// Calculates routes between destinations using Haversine formula based on lat/lng coordinates.
    /// This avoids making additional API calls for inter-destination distances.
    /// </summary>
    /// <param name="destinations">The collection of destination addresses with coordinate information.</param>
    /// <returns>A list of route information between all destination pairs.</returns>
    private List<RouteInfo> CalculateDestinationToDestinationRoutes(List<AddressLocation> destinations)
    {
        var routes = new List<RouteInfo>();

        for (int i = 0; i < destinations.Count; i++)
        {
            for (int j = 0; j < destinations.Count; j++)
            {
                // Skip same-to-same routes
                if (i == j) continue;

                var origin = destinations[i];
                var destination = destinations[j];

                // Ensure we have coordinates
                if (!origin.Lat.HasValue || !origin.Lng.HasValue || 
                    !destination.Lat.HasValue || !destination.Lng.HasValue)
                {
                    _logger.LogWarning("Missing coordinates for destination-to-destination route calculation");
                    continue;
                }

                // Calculate distance using Haversine formula
                var distanceMeters = CalculateHaversineDistance(
                    origin.Lat.Value, origin.Lng.Value,
                    destination.Lat.Value, destination.Lng.Value);

                // Estimate duration assuming average speed of 50 km/h (13.89 m/s)
                var durationSeconds = (int)(distanceMeters / 13.89);

                routes.Add(new RouteInfo
                {
                    OriginIndex = i,
                    DestinationIndex = j,
                    OriginAddress = origin.Address ?? origin.PlaceId ?? "Unknown",
                    DestinationAddress = destination.Address ?? destination.PlaceId ?? "Unknown",
                    DistanceMeters = distanceMeters,
                    Duration = $"{durationSeconds}s",
                    Status = "OK",
                    OriginLocation = new LatLng { Lat = origin.Lat.Value, Lng = origin.Lng.Value },
                    DestinationLocation = new LatLng { Lat = destination.Lat.Value, Lng = destination.Lng.Value }
                });
            }
        }

        return routes;
    }

    /// <summary>
    /// Calculates the great-circle distance between two points on Earth using the Haversine formula.
    /// </summary>
    /// <param name="lat1">The latitude of the first point in decimal degrees.</param>
    /// <param name="lon1">The longitude of the first point in decimal degrees.</param>
    /// <param name="lat2">The latitude of the second point in decimal degrees.</param>
    /// <param name="lon2">The longitude of the second point in decimal degrees.</param>
    /// <returns>The distance between the two points in meters.</returns>
    private int CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth's radius in meters

        var lat1Rad = lat1 * Math.PI / 180;
        var lat2Rad = lat2 * Math.PI / 180;
        var deltaLat = (lat2 - lat1) * Math.PI / 180;
        var deltaLon = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        var distance = R * c;

        return (int)Math.Round(distance);
    }
}
