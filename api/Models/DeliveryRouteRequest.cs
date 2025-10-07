namespace api.Models;

/// <summary>
/// Represents a delivery route optimization request containing origins, destinations, and optimization parameters.
/// </summary>
public class DeliveryRouteRequest
{
    /// <summary>
    /// Gets or sets the collection of origin addresses for delivery routes.
    /// </summary>
    /// <value>A list of address locations representing starting points for deliveries. The default is an empty list.</value>
    public List<AddressLocation> Origins { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of destination addresses for delivery routes.
    /// </summary>
    /// <value>A list of address locations representing delivery destinations. The default is an empty list.</value>
    public List<AddressLocation> Destinations { get; set; } = new();

    /// <summary>
    /// Gets or sets the optimization algorithm to use for route calculation.
    /// </summary>
    /// <value>One of the enumeration values that specifies the optimization algorithm. The default is <see cref="OptimizationAlgorithm.NearestNeighbor"/>.</value>
    public OptimizationAlgorithm Algorithm { get; set; } = OptimizationAlgorithm.NearestNeighbor;

    /// <summary>
    /// Gets or sets the vehicle emission type for route calculation.
    /// </summary>
    /// <value>One of the enumeration values that specifies the vehicle emission type. The default is <see cref="VehicleEmissionType.Gasoline"/>.</value>
    public VehicleEmissionType VehicleEmissionType { get; set; } = VehicleEmissionType.Gasoline;
}

/// <summary>
/// Specifies the optimization algorithm used for route calculation.
/// </summary>
public enum OptimizationAlgorithm
{
    /// <summary>
    /// Uses the Nearest Neighbor algorithm for route optimization.
    /// </summary>
    NearestNeighbor,

    /// <summary>
    /// Uses the A* algorithm for route optimization.
    /// </summary>
    AStar
}

/// <summary>
/// Specifies the vehicle emission type for environmental impact calculation.
/// </summary>
public enum VehicleEmissionType
{
    /// <summary>
    /// Represents a gasoline-powered vehicle.
    /// </summary>
    Gasoline,

    /// <summary>
    /// Represents an electric-powered vehicle.
    /// </summary>
    Electric,

    /// <summary>
    /// Represents a hybrid-powered vehicle.
    /// </summary>
    Hybrid,

    /// <summary>
    /// Represents a diesel-powered vehicle.
    /// </summary>
    Diesel
}

/// <summary>
/// Represents a geographic location with address and coordinate information.
/// </summary>
public class AddressLocation
{
    /// <summary>
    /// Gets or sets the human-readable address string.
    /// </summary>
    /// <value>A string containing the formatted address, or <see langword="null"/> if not available.</value>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the Google Places API place identifier.
    /// </summary>
    /// <value>A string containing the Google Places place ID, or <see langword="null"/> if not available.</value>
    public string? PlaceId { get; set; }

    /// <summary>
    /// Gets or sets the latitude coordinate.
    /// </summary>
    /// <value>A double representing the latitude in decimal degrees, or <see langword="null"/> if not available.</value>
    public double? Lat { get; set; }

    /// <summary>
    /// Gets or sets the longitude coordinate.
    /// </summary>
    /// <value>A double representing the longitude in decimal degrees, or <see langword="null"/> if not available.</value>
    public double? Lng { get; set; }
}
