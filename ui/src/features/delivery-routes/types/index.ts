// Place-related types
export interface PlaceSelectEvent extends Event {
  place: {
    id: string;
    displayName?: string;
    formattedAddress?: string;
    location?: {
      lat: () => number;
      lng: () => number;
    };
    fetchFields: (options: { fields: string[] }) => Promise<void>;
  };
}

export interface SelectedPlace {
  id: string;
  address?: string;
  placeId: string;
  lat?: number;
  lng?: number;
}

export interface Location {
  address?: string;
  placeId?: string;
  lat?: number;
  lng?: number;
}

export interface LatLng {
  lat: number;
  lng: number;
}

// Route-related types
export type Algorithm = "NearestNeighbor" | "AStar";
export type VehicleEmissionType = "Gasoline" | "Electric" | "Hybrid" | "Diesel";

export interface DeliveryRouteRequest {
  origins: Location[];
  destinations: Location[];
  algorithm?: Algorithm;
  vehicleEmissionType?: VehicleEmissionType;
}

export interface Route {
  originIndex: number;
  destinationIndex: number;
  originAddress: string;
  destinationAddress: string;
  distanceMeters: number;
  duration: string;
  status: string;
  originLocation?: LatLng;
  destinationLocation?: LatLng;
}

export interface OptimizedRoute {
  originIndex: number;
  destinationIndex: number;
  order: number;
  route: Route;
}

export interface OptimizedOriginSequence {
  originIndex: number;
  originAddress: string;
  routeSequence: Route[];
  totalDistanceMeters: number;
  totalDuration: string;
  destinationsVisited: number;
}

export interface DeliveryRouteResponse {
  routes: Route[];
  optimizedSequences: OptimizedOriginSequence[];
  totalDistanceMeters: number;
  totalDuration: string;
  calculationTimeMs: number;
}
