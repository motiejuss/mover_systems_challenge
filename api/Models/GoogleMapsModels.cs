using System.Text.Json.Serialization;

namespace api.Models;

/// <summary>
/// Represents an origin point in a Google Maps route matrix request.
/// </summary>
public class RouteMatrixOrigin
{
    /// <summary>
    /// Gets or sets the waypoint information for the origin.
    /// </summary>
    /// <value>A waypoint containing address or place ID information. The default is a new instance.</value>
    [JsonPropertyName("waypoint")]
    public Waypoint Waypoint { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the optional route modifiers for this origin.
    /// </summary>
    /// <value>Route modifiers containing vehicle information, or <see langword="null"/> if not specified.</value>
    [JsonPropertyName("routeModifiers")]
    public RouteModifiers? RouteModifiers { get; set; }
}

/// <summary>
/// Represents a destination point in a Google Maps route matrix request.
/// </summary>
public class RouteMatrixDestination
{
    /// <summary>
    /// Gets or sets the waypoint information for the destination.
    /// </summary>
    /// <value>A waypoint containing address or place ID information. The default is a new instance.</value>
    [JsonPropertyName("waypoint")]
    public Waypoint Waypoint { get; set; } = new();
}

/// <summary>
/// Represents a waypoint with address or place identifier information.
/// </summary>
public class Waypoint
{
    /// <summary>
    /// Gets or sets the human-readable address string.
    /// </summary>
    /// <value>A string containing the formatted address, or <see langword="null"/> if using place ID.</value>
    [JsonPropertyName("address")]
    public string? Address { get; set; }
    
    /// <summary>
    /// Gets or sets the Google Places API place identifier.
    /// </summary>
    /// <value>A string containing the Google Places place ID, or <see langword="null"/> if using address.</value>
    [JsonPropertyName("placeId")]
    public string? PlaceId { get; set; }
}

/// <summary>
/// Represents a request to the Google Maps route matrix API.
/// </summary>
public class RouteMatrixRequest
{
    /// <summary>
    /// Gets or sets the collection of origin points.
    /// </summary>
    /// <value>A list of origin waypoints for route calculation. The default is an empty list.</value>
    [JsonPropertyName("origins")]
    public List<RouteMatrixOrigin> Origins { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the collection of destination points.
    /// </summary>
    /// <value>A list of destination waypoints for route calculation. The default is an empty list.</value>
    [JsonPropertyName("destinations")]
    public List<RouteMatrixDestination> Destinations { get; set; } = new();
}

/// <summary>
/// Represents route modifiers that affect route calculation.
/// </summary>
public class RouteModifiers
{
    /// <summary>
    /// Gets or sets the vehicle information for route calculation.
    /// </summary>
    /// <value>Vehicle information including emission type, or <see langword="null"/> if not specified.</value>
    [JsonPropertyName("vehicleInfo")]
    public VehicleInfo? VehicleInfo { get; set; }
}

/// <summary>
/// Represents vehicle information for route calculation.
/// </summary>
public class VehicleInfo
{
    /// <summary>
    /// Gets or sets the vehicle emission type.
    /// </summary>
    /// <value>A string representing the emission type (e.g., "GASOLINE", "ELECTRIC"), or <see langword="null"/> if not specified.</value>
    [JsonPropertyName("emissionType")]
    public string? EmissionType { get; set; }
}

/// <summary>
/// Represents a response from the Google Maps route matrix API.
/// </summary>
public class RouteMatrixResponse
{
    /// <summary>
    /// Gets or sets the index of the origin in the request.
    /// </summary>
    /// <value>An integer representing the zero-based index of the origin.</value>
    [JsonPropertyName("originIndex")]
    public int OriginIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the index of the destination in the request.
    /// </summary>
    /// <value>An integer representing the zero-based index of the destination.</value>
    [JsonPropertyName("destinationIndex")]
    public int DestinationIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the status of the route calculation.
    /// </summary>
    /// <value>A status object containing error information, or <see langword="null"/> if successful.</value>
    [JsonPropertyName("status")]
    public Status? Status { get; set; }
    
    /// <summary>
    /// Gets or sets the distance in meters between origin and destination.
    /// </summary>
    /// <value>An integer representing the distance in meters.</value>
    [JsonPropertyName("distanceMeters")]
    public int DistanceMeters { get; set; }
    
    /// <summary>
    /// Gets or sets the duration of the route.
    /// </summary>
    /// <value>A string representing the duration (e.g., "123s"), or <see langword="null"/> if unavailable.</value>
    [JsonPropertyName("duration")]
    public string? Duration { get; set; }
}

/// <summary>
/// Represents the status of a route calculation request.
/// </summary>
public class Status
{
    /// <summary>
    /// Gets or sets the status code.
    /// </summary>
    /// <value>An integer representing the status code (0 for success).</value>
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    /// <value>A string containing the status message, or <see langword="null"/> if not provided.</value>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>
/// Represents a response containing delivery route information.
/// </summary>
public class DeliveryRouteResponse
{
    /// <summary>
    /// Gets or sets the collection of route information.
    /// </summary>
    /// <value>A list of route details between origins and destinations. The default is an empty list.</value>
    public List<RouteInfo> Routes { get; set; } = new();
}

/// <summary>
/// Represents detailed information about a route between two points.
/// </summary>
public class RouteInfo
{
    /// <summary>
    /// Gets or sets the index of the origin point.
    /// </summary>
    /// <value>An integer representing the zero-based index of the origin.</value>
    public int OriginIndex { get; set; }

    /// <summary>
    /// Gets or sets the index of the destination point.
    /// </summary>
    /// <value>An integer representing the zero-based index of the destination.</value>
    public int DestinationIndex { get; set; }

    /// <summary>
    /// Gets or sets the human-readable address of the origin.
    /// </summary>
    /// <value>A string containing the origin address. The default is an empty string.</value>
    public string OriginAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable address of the destination.
    /// </summary>
    /// <value>A string containing the destination address. The default is an empty string.</value>
    public string DestinationAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the distance in meters between origin and destination.
    /// </summary>
    /// <value>An integer representing the distance in meters.</value>
    public int DistanceMeters { get; set; }

    /// <summary>
    /// Gets or sets the duration of the route.
    /// </summary>
    /// <value>A string representing the duration. The default is an empty string.</value>
    public string Duration { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status of the route calculation.
    /// </summary>
    /// <value>A string representing the route status. The default is an empty string.</value>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the geographic coordinates of the origin.
    /// </summary>
    /// <value>Latitude and longitude coordinates of the origin, or <see langword="null"/> if not available.</value>
    public LatLng? OriginLocation { get; set; }

    /// <summary>
    /// Gets or sets the geographic coordinates of the destination.
    /// </summary>
    /// <value>Latitude and longitude coordinates of the destination, or <see langword="null"/> if not available.</value>
    public LatLng? DestinationLocation { get; set; }
}

/// <summary>
/// Represents latitude and longitude coordinates.
/// </summary>
public class LatLng
{
    /// <summary>
    /// Gets or sets the latitude coordinate.
    /// </summary>
    /// <value>A double representing the latitude in decimal degrees.</value>
    public double Lat { get; set; }

    /// <summary>
    /// Gets or sets the longitude coordinate.
    /// </summary>
    /// <value>A double representing the longitude in decimal degrees.</value>
    public double Lng { get; set; }
}

/// <summary>
/// Represents an optimized delivery route response with calculated sequences and totals.
/// </summary>
public class OptimizedRouteResponse
{
    /// <summary>
    /// Gets or sets the collection of all route information.
    /// </summary>
    /// <value>A list containing all route details used in optimization. The default is an empty list.</value>
    public List<RouteInfo> Routes { get; set; } = new();

    /// <summary>
    /// Gets or sets the optimized delivery sequences for each origin.
    /// </summary>
    /// <value>A list of optimized route sequences, one per origin. The default is an empty list.</value>
    public List<OptimizedOriginSequence> OptimizedSequences { get; set; } = new();

    /// <summary>
    /// Gets or sets the total distance across all optimized routes.
    /// </summary>
    /// <value>An integer representing the total distance in meters.</value>
    public int TotalDistanceMeters { get; set; }

    /// <summary>
    /// Gets or sets the total duration across all optimized routes.
    /// </summary>
    /// <value>A string representing the total duration. The default is an empty string.</value>
    public string TotalDuration { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the calculation time for the optimization algorithm.
    /// </summary>
    /// <value>A double representing the calculation time in milliseconds.</value>
    public double CalculationTimeMs { get; set; }
}

/// <summary>
/// Represents an optimized delivery sequence for a single origin point.
/// </summary>
public class OptimizedOriginSequence
{
    /// <summary>
    /// Gets or sets the index of the origin point.
    /// </summary>
    /// <value>An integer representing the zero-based index of the origin.</value>
    public int OriginIndex { get; set; }

    /// <summary>
    /// Gets or sets the human-readable address of the origin.
    /// </summary>
    /// <value>A string containing the origin address. The default is an empty string.</value>
    public string OriginAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optimized sequence of routes from this origin.
    /// </summary>
    /// <value>A list of routes in optimal visiting order. The default is an empty list.</value>
    public List<RouteInfo> RouteSequence { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of destinations visited from this origin.
    /// </summary>
    /// <value>An integer representing the count of destinations visited.</value>
    public int DestinationsVisited { get; set; }
}
