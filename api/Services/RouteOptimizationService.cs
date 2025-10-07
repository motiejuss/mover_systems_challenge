using api.Models;

namespace api.Services;

/// <summary>
/// Provides route optimization functionality using various algorithms including Nearest Neighbor and A*.
/// </summary>
public class RouteOptimizationService
{
    private readonly ILogger<RouteOptimizationService> _logger;

    /// <summary>
    /// Initializes a new instance of the RouteOptimizationService class.
    /// </summary>
    /// <param name="logger">The logger for recording optimization operations.</param>
    public RouteOptimizationService(ILogger<RouteOptimizationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Optimizes routes using the specified algorithm to find the optimal order to visit destinations from each origin.
    /// </summary>
    /// <param name="allRoutes">The collection of all available routes between origins and destinations.</param>
    /// <param name="originCount">The number of origin points.</param>
    /// <param name="destinationCount">The number of destination points.</param>
    /// <param name="algorithm">One of the enumeration values that specifies the optimization algorithm to use. The default is <see cref="OptimizationAlgorithm.NearestNeighbor"/>.</param>
    /// <returns>An optimized route response containing the best sequences and total metrics.</returns>
    public OptimizedRouteResponse OptimizeRoutes(List<RouteInfo> allRoutes, int originCount, int destinationCount, OptimizationAlgorithm algorithm = OptimizationAlgorithm.NearestNeighbor)
    {
        var optimizedSequences = new List<OptimizedOriginSequence>();

        // Process each origin separately
        for (int originIndex = 0; originIndex < originCount; originIndex++)
        {
            var sequence = algorithm == OptimizationAlgorithm.AStar 
                ? OptimizeForOriginAStar(allRoutes, originIndex, destinationCount)
                : OptimizeForOrigin(allRoutes, originIndex, destinationCount);
            optimizedSequences.Add(sequence);
        }

        // Calculate overall totals across all origins
        var totalDistance = optimizedSequences.Sum(s => s.RouteSequence.Sum(r => r.DistanceMeters));
        var totalDurationSeconds = optimizedSequences.Sum(s => s.RouteSequence.Sum(r => ParseDuration(r.Duration)));

        return new OptimizedRouteResponse
        {
            Routes = allRoutes,
            OptimizedSequences = optimizedSequences,
            TotalDistanceMeters = totalDistance,
            TotalDuration = $"{totalDurationSeconds:F0}s"
        };
    }

    /// <summary>
    /// Applies Nearest Neighbor Algorithm for a single origin to find the optimal visitation sequence.
    /// </summary>
    /// <param name="allRoutes">The collection of all available routes between locations.</param>
    /// <param name="originIndex">The index of the origin point to optimize from.</param>
    /// <param name="destinationCount">The total number of destinations to visit.</param>
    /// <returns>An optimized origin sequence containing the best route order and metrics.</returns>
    private OptimizedOriginSequence OptimizeForOrigin(List<RouteInfo> allRoutes, int originIndex, int destinationCount)
    {
        var visitedDestinations = new HashSet<int>();
        var routeSequence = new List<RouteInfo>();
        int? currentLocationIndex = null; // null = at origin, otherwise = destination index
        string currentAddress = "";
        LatLng? currentLocation = null;
        var totalDistance = 0;
        var totalDurationSeconds = 0.0;

        // Get the origin address from routes that start from this origin
        var originRoute = allRoutes.FirstOrDefault(r => r.OriginIndex == originIndex);
        var originAddress = originRoute?.OriginAddress ?? "";
        currentAddress = originAddress;
        currentLocation = originRoute?.OriginLocation;

        // Visit all destinations using Nearest Neighbor
        while (visitedDestinations.Count < destinationCount)
        {
            RouteInfo? nearestRoute = null;
            int nearestDistance = int.MaxValue;

            // Find the nearest unvisited destination from current location
            foreach (var route in allRoutes)
            {
                bool isValidRoute;
                
                if (currentLocationIndex == null)
                {
                    // At origin: match by origin location coordinates to avoid confusion with dest-to-dest routes
                    // Dest-to-dest routes have different origin locations (at destination coords)
                    isValidRoute = route.OriginIndex == originIndex && 
                                  route.OriginLocation != null && 
                                  currentLocation != null &&
                                  Math.Abs(route.OriginLocation.Lat - currentLocation.Lat) < 0.0001 &&
                                  Math.Abs(route.OriginLocation.Lng - currentLocation.Lng) < 0.0001;
                }
                else
                {
                    // At a destination: match by current location coordinates
                    isValidRoute = route.OriginLocation != null && 
                                  currentLocation != null &&
                                  Math.Abs(route.OriginLocation.Lat - currentLocation.Lat) < 0.0001 &&
                                  Math.Abs(route.OriginLocation.Lng - currentLocation.Lng) < 0.0001;
                }

                // Check if destination hasn't been visited yet
                if (isValidRoute && !visitedDestinations.Contains(route.DestinationIndex) && route.Status == "OK")
                {
                    if (route.DistanceMeters < nearestDistance)
                    {
                        nearestDistance = route.DistanceMeters;
                        nearestRoute = route;
                    }
                }
            }

            if (nearestRoute != null)
            {
                // Create a route segment that represents the actual travel from current location to next destination
                var routeSegment = new RouteInfo
                {
                    OriginIndex = currentLocationIndex ?? originIndex,
                    DestinationIndex = nearestRoute.DestinationIndex,
                    OriginAddress = currentAddress,
                    DestinationAddress = nearestRoute.DestinationAddress,
                    DistanceMeters = nearestRoute.DistanceMeters,
                    Duration = nearestRoute.Duration,
                    Status = nearestRoute.Status,
                    OriginLocation = currentLocation,
                    DestinationLocation = nearestRoute.DestinationLocation
                };
                
                routeSequence.Add(routeSegment);
                visitedDestinations.Add(nearestRoute.DestinationIndex);
                currentLocationIndex = nearestRoute.DestinationIndex;
                currentAddress = nearestRoute.DestinationAddress;
                currentLocation = nearestRoute.DestinationLocation;
                totalDistance += nearestRoute.DistanceMeters;
                totalDurationSeconds += ParseDuration(nearestRoute.Duration);
            }
            else
            {
                // No valid route found, break to avoid infinite loop
                _logger.LogWarning("No valid route found for origin {OriginIndex}, visited {Count} destinations", 
                    originIndex, visitedDestinations.Count);
                break;
            }
        }

        return new OptimizedOriginSequence
        {
            OriginIndex = originIndex,
            OriginAddress = originAddress,
            RouteSequence = routeSequence,
            DestinationsVisited = visitedDestinations.Count
        };
    }

    /// <summary>
    /// Applies A* Algorithm for a single origin to find the globally optimal visitation sequence.
    /// </summary>
    /// <param name="allRoutes">The collection of all available routes between locations.</param>
    /// <param name="originIndex">The index of the origin point to optimize from.</param>
    /// <param name="destinationCount">The total number of destinations to visit.</param>
    /// <returns>An optimized origin sequence containing the globally optimal route order and metrics.</returns>
    private OptimizedOriginSequence OptimizeForOriginAStar(List<RouteInfo> allRoutes, int originIndex, int destinationCount)
    {
        // Get the origin address from routes that start from this origin
        var originAddress = allRoutes
            .FirstOrDefault(r => r.OriginIndex == originIndex)?.OriginAddress ?? "";
        
        // Create a state class to track paths through the search space
        var bestPath = FindBestPathAStar(allRoutes, originIndex, destinationCount);
        
        return new OptimizedOriginSequence
        {
            OriginIndex = originIndex,
            OriginAddress = originAddress,
            RouteSequence = bestPath,
            DestinationsVisited = bestPath.Count
        };
    }

    /// <summary>
    /// Finds the best path using optimized A* search algorithm with state deduplication and improved heuristic.
    /// For large numbers of destinations (>12), uses a hybrid approach with bounded search.
    /// </summary>
    /// <param name="allRoutes">The collection of all available routes between locations.</param>
    /// <param name="originIndex">The index of the origin point to start the search from.</param>
    /// <param name="destinationCount">The total number of destinations to visit.</param>
    /// <returns>A list of route information representing the optimal path sequence.</returns>
    private List<RouteInfo> FindBestPathAStar(List<RouteInfo> allRoutes, int originIndex, int destinationCount)
    {
        // Get origin information
        var originRoute = allRoutes.FirstOrDefault(r => r.OriginIndex == originIndex);
        var originAddress = originRoute?.OriginAddress ?? "";
        var originLocation = originRoute?.OriginLocation;
        
        // For very large destination counts, use a bounded search with beam width
        const int BEAM_WIDTH_THRESHOLD = 12;
        int beamWidth = destinationCount > BEAM_WIDTH_THRESHOLD ? 1000 : int.MaxValue;
        
        // Pre-build distance lookup for faster access
        var distanceLookup = BuildDistanceLookup(allRoutes, originIndex);
        
        // State represents a path through destinations
        var openSet = new PriorityQueue<AStarState, int>();
        
        // Track visited states to avoid duplicate exploration: (currentLocation, visitedSet) -> bestCost
        var visitedStates = new Dictionary<string, int>();
        
        var initialState = new AStarState
        {
            VisitedDestinations = new HashSet<int>(),
            Path = new List<RouteInfo>(),
            CurrentCost = 0,
            CurrentLocationIndex = null, // null means at origin
            CurrentAddress = originAddress,
            CurrentLocation = originLocation
        };
        
        // Calculate heuristic for initial state using MST-based heuristic
        var initialHeuristic = CalculateMSTHeuristic(distanceLookup, null, initialState.VisitedDestinations, destinationCount);
        openSet.Enqueue(initialState, initialHeuristic);
        
        AStarState? bestCompleteState = null;
        var bestCompleteCost = int.MaxValue;
        
        // Track states at current depth for beam search
        var currentDepthStates = new List<(AStarState state, int priority)>();
        int currentDepth = 0;
        
        while (openSet.Count > 0)
        {
            var currentState = openSet.Dequeue();
            
            // If we've visited all destinations, this is a complete path
            if (currentState.VisitedDestinations.Count == destinationCount)
            {
                if (currentState.CurrentCost < bestCompleteCost)
                {
                    bestCompleteCost = currentState.CurrentCost;
                    bestCompleteState = currentState;
                    _logger.LogInformation("Found better solution: {Cost} meters", bestCompleteCost);
                }
                continue;
            }
            
            // Create state key for deduplication
            var stateKey = CreateStateKey(currentState.CurrentLocationIndex, currentState.VisitedDestinations);
            
            // Skip if we've already explored this state with a better cost
            if (visitedStates.TryGetValue(stateKey, out var previousCost) && previousCost <= currentState.CurrentCost)
            {
                continue;
            }
            visitedStates[stateKey] = currentState.CurrentCost;
            
            // Get available routes from current location
            var availableRoutes = GetAvailableRoutes(allRoutes, originIndex, currentState, distanceLookup);
            
            foreach (var route in availableRoutes)
            {
                var newCost = currentState.CurrentCost + route.DistanceMeters;
                
                // Early pruning: skip if already worse than best complete solution
                if (newCost >= bestCompleteCost)
                    continue;
                
                // Create a route segment
                var routeSegment = new RouteInfo
                {
                    OriginIndex = currentState.CurrentLocationIndex ?? originIndex,
                    DestinationIndex = route.DestinationIndex,
                    OriginAddress = currentState.CurrentAddress,
                    DestinationAddress = route.DestinationAddress,
                    DistanceMeters = route.DistanceMeters,
                    Duration = route.Duration,
                    Status = route.Status,
                    OriginLocation = currentState.CurrentLocation,
                    DestinationLocation = route.DestinationLocation
                };
                
                var newVisited = new HashSet<int>(currentState.VisitedDestinations) { route.DestinationIndex };
                var newPath = new List<RouteInfo>(currentState.Path) { routeSegment };
                
                var newState = new AStarState
                {
                    VisitedDestinations = newVisited,
                    Path = newPath,
                    CurrentCost = newCost,
                    CurrentLocationIndex = route.DestinationIndex,
                    CurrentAddress = route.DestinationAddress,
                    CurrentLocation = route.DestinationLocation
                };
                
                // Calculate f = g + h (total cost + MST heuristic)
                var heuristic = CalculateMSTHeuristic(distanceLookup, route.DestinationIndex, newVisited, destinationCount);
                var totalEstimatedCost = newCost + heuristic;
                
                // Prune if estimated cost is worse than best complete path
                if (totalEstimatedCost < bestCompleteCost)
                {
                    // For beam search, collect states at this depth
                    if (beamWidth < int.MaxValue && newState.VisitedDestinations.Count > currentDepth)
                    {
                        currentDepthStates.Add((newState, totalEstimatedCost));
                        currentDepth = newState.VisitedDestinations.Count;
                    }
                    else
                    {
                        openSet.Enqueue(newState, totalEstimatedCost);
                    }
                }
            }
            
            // Apply beam search pruning if we've moved to next depth level
            if (beamWidth < int.MaxValue && currentDepthStates.Count > 0 && 
                (openSet.Count == 0 || openSet.Peek().VisitedDestinations.Count > currentDepth))
            {
                // Keep only top beamWidth states
                var topStates = currentDepthStates
                    .OrderBy(x => x.priority)
                    .Take(beamWidth)
                    .ToList();
                
                foreach (var (state, priority) in topStates)
                {
                    openSet.Enqueue(state, priority);
                }
                
                currentDepthStates.Clear();
            }
        }
        
        return bestCompleteState?.Path ?? new List<RouteInfo>();
    }
    
    /// <summary>
    /// Builds a distance lookup dictionary for O(1) access to distances between locations.
    /// </summary>
    /// <param name="allRoutes">The collection of all available routes to build the lookup from.</param>
    /// <param name="originIndex">The index of the primary origin point for filtering routes.</param>
    /// <returns>A dictionary mapping location pairs to route information for fast access.</returns>
    private Dictionary<(int?, int), RouteInfo> BuildDistanceLookup(List<RouteInfo> allRoutes, int originIndex)
    {
        var lookup = new Dictionary<(int?, int), RouteInfo>();
        
        foreach (var route in allRoutes.Where(r => r.Status == "OK"))
        {
            var key = (route.OriginIndex == originIndex && route.OriginLocation != null ? 
                      (int?)null : 
                      route.OriginIndex, 
                      route.DestinationIndex);
            
            if (!lookup.ContainsKey(key))
            {
                lookup[key] = route;
            }
        }
        
        return lookup;
    }
    
    /// <summary>
    /// Creates a unique key for state deduplication based on current location and visited destinations.
    /// </summary>
    /// <param name="currentLocationIndex">The index of the current location, or <see langword="null"/> if at origin.</param>
    /// <param name="visitedDestinations">The set of destination indices that have already been visited.</param>
    /// <returns>A unique string key representing the current state for deduplication purposes.</returns>
    private string CreateStateKey(int? currentLocationIndex, HashSet<int> visitedDestinations)
    {
        var visited = string.Join(",", visitedDestinations.OrderBy(x => x));
        return $"{currentLocationIndex?.ToString() ?? "origin"}:{visited}";
    }
    
    /// <summary>
    /// Gets available routes from current location using pre-built lookup.
    /// </summary>
    /// <param name="allRoutes">The collection of all available routes (used for fallback lookup).</param>
    /// <param name="originIndex">The index of the primary origin point.</param>
    /// <param name="currentState">The current state containing location and visited destinations information.</param>
    /// <param name="distanceLookup">The pre-built lookup dictionary for fast route access.</param>
    /// <returns>A list of available routes from the current location to unvisited destinations.</returns>
    private List<RouteInfo> GetAvailableRoutes(List<RouteInfo> allRoutes, int originIndex, AStarState currentState, Dictionary<(int?, int), RouteInfo> distanceLookup)
    {
        var availableRoutes = new List<RouteInfo>();
        
        // Get all unvisited destinations
        var unvisitedDestinations = Enumerable.Range(0, distanceLookup.Keys.Max(k => k.Item2) + 1)
            .Where(d => !currentState.VisitedDestinations.Contains(d))
            .ToList();
        
        foreach (var destIndex in unvisitedDestinations)
        {
            var key = (currentState.CurrentLocationIndex, destIndex);
            if (distanceLookup.TryGetValue(key, out var route))
            {
                availableRoutes.Add(route);
            }
        }
        
        return availableRoutes;
    }
    
    /// <summary>
    /// Calculates improved heuristic using Minimum Spanning Tree of unvisited destinations.
    /// This provides a better lower bound than simple min-distance heuristic.
    /// </summary>
    private int CalculateMSTHeuristic(Dictionary<(int?, int), RouteInfo> distanceLookup, int? currentLocationIndex, HashSet<int> visitedDestinations, int destinationCount)
    {
        if (visitedDestinations.Count == destinationCount)
            return 0;
        
        // Get unvisited destinations
        var unvisited = Enumerable.Range(0, destinationCount)
            .Where(d => !visitedDestinations.Contains(d))
            .ToList();
        
        if (unvisited.Count == 0)
            return 0;
        
        if (unvisited.Count == 1)
        {
            // Only one destination left, return direct distance
            var key = (currentLocationIndex, unvisited[0]);
            return distanceLookup.TryGetValue(key, out var route) ? route.DistanceMeters : 0;
        }
        
        // Find minimum distance from current location to any unvisited destination
        var minToUnvisited = int.MaxValue;
        foreach (var dest in unvisited)
        {
            var key = (currentLocationIndex, dest);
            if (distanceLookup.TryGetValue(key, out var route))
            {
                minToUnvisited = Math.Min(minToUnvisited, route.DistanceMeters);
            }
        }
        
        // Calculate MST of unvisited destinations using Prim's algorithm (simplified)
        var mstCost = 0;
        var inMST = new HashSet<int> { unvisited[0] };
        var remaining = new HashSet<int>(unvisited.Skip(1));
        
        while (remaining.Count > 0)
        {
            var minEdge = int.MaxValue;
            var minDest = -1;
            
            // Find minimum edge connecting MST to remaining nodes
            foreach (var inNode in inMST)
            {
                foreach (var outNode in remaining)
                {
                    // Try both directions to find distance
                    var key1 = ((int?)inNode, outNode);
                    var key2 = ((int?)outNode, inNode);
                    
                    var distance = int.MaxValue;
                    if (distanceLookup.TryGetValue(key1, out var route1))
                        distance = route1.DistanceMeters;
                    else if (distanceLookup.TryGetValue(key2, out var route2))
                        distance = route2.DistanceMeters;
                    
                    if (distance < minEdge)
                    {
                        minEdge = distance;
                        minDest = outNode;
                    }
                }
            }
            
            if (minDest >= 0)
            {
                mstCost += minEdge;
                inMST.Add(minDest);
                remaining.Remove(minDest);
            }
            else
            {
                break; // No connection found
            }
        }
        
        // Total heuristic: distance to nearest unvisited + MST of unvisited
        return (minToUnvisited == int.MaxValue ? 0 : minToUnvisited) + mstCost;
    }

    /// <summary>
    /// Calculates the heuristic (h) for A* - estimated cost to visit all remaining destinations.
    /// Uses the minimum distance to any unvisited destination as an optimistic estimate.
    /// </summary>
    private int CalculateHeuristicFromLocation(List<RouteInfo> allRoutes, int originIndex, int? currentLocationIndex, HashSet<int> visitedDestinations, int destinationCount)
    {
        if (visitedDestinations.Count == destinationCount)
            return 0;
        
        // Determine which location we're calculating from
        int locationIndex = currentLocationIndex ?? originIndex;
        
        // Find the minimum distance to any unvisited destination from current location
        var minDistance = allRoutes
            .Where(r => r.OriginIndex == locationIndex && 
                       !visitedDestinations.Contains(r.DestinationIndex) &&
                       r.Status == "OK")
            .Select(r => r.DistanceMeters)
            .DefaultIfEmpty(0)
            .Min();
        
        // Multiply by remaining destinations as an admissible heuristic
        var remainingDestinations = destinationCount - visitedDestinations.Count;
        return minDistance * remainingDestinations;
    }

    /// <summary>
    /// Parses duration string (e.g., "6272s") to seconds.
    /// </summary>
    private double ParseDuration(string duration)
    {
        if (string.IsNullOrEmpty(duration) || duration == "N/A")
            return 0;

        // Remove 's' suffix and parse
        var durationStr = duration.TrimEnd('s');
        if (double.TryParse(durationStr, out var seconds))
            return seconds;

        return 0;
    }

    /// <summary>
    /// Represents a state in the A* search space.
    /// </summary>
    private class AStarState
    {
        public HashSet<int> VisitedDestinations { get; set; } = new();
        public List<RouteInfo> Path { get; set; } = new();
        public int CurrentCost { get; set; }
        public int? CurrentLocationIndex { get; set; } // null = at origin, otherwise = destination index
        public string CurrentAddress { get; set; } = string.Empty;
        public LatLng? CurrentLocation { get; set; }
    }
}
