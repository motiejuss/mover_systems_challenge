import React, { useState, useCallback, useEffect } from "react";
import { GoogleMap, Marker, Polyline } from "@react-google-maps/api";
import { MAPS_CONFIG } from "../../../../shared/config/maps.config";
import { getRouteColor } from "../../utils/route.utils";
import type { SelectedPlace, DeliveryRouteResponse, LatLng } from "../../types";

interface RouteMapProps {
  origin: SelectedPlace | null;
  routes: DeliveryRouteResponse | null;
}

export const RouteMap: React.FC<RouteMapProps> = ({ origin, routes }) => {
  const [map, setMap] = useState<google.maps.Map | null>(null);

  const svgMarker = {
    path: "M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z",
    fillColor: "black",
    fillOpacity: 1,
    strokeWeight: 0,
    rotation: 0,
    scale: 1.5,
    anchor: new google.maps.Point(10, 10),
  };

  const getUniqueLocations = useCallback(() => {
    if (!routes?.routes) return { origin: null, destinations: [] };

    const destinationMap = new Map<
      number,
      { location: LatLng; sequenceNumber?: number }
    >();

    if (routes.optimizedSequences && routes.optimizedSequences.length > 0) {
      routes.optimizedSequences.forEach((sequence) => {
        sequence.routeSequence.forEach((route, seqIndex) => {
          if (route.destinationLocation) {
            destinationMap.set(route.destinationIndex, {
              location: route.destinationLocation,
              sequenceNumber: seqIndex + 1,
            });
          }
        });
      });
    } else {
      routes.routes.forEach((route) => {
        if (route.destinationLocation) {
          destinationMap.set(route.destinationIndex, {
            location: route.destinationLocation,
          });
        }
      });
    }

    return {
      origin:
        origin && origin.lat && origin.lng
          ? { lat: origin.lat, lng: origin.lng }
          : null,
      destinations: Array.from(destinationMap.entries()).map(
        ([index, data]) => ({
          index,
          location: data.location,
          sequenceNumber: data.sequenceNumber || index + 1,
        })
      ),
    };
  }, [origin, routes]);

  const getPolylinePaths = useCallback(() => {
    // Explicitly return empty array when routes is null or undefined
    if (!routes) {
      return [];
    }

    if (!routes.optimizedSequences || routes.optimizedSequences.length === 0) {
      return [];
    }

    const paths: Array<{
      path: Array<{ lat: number; lng: number }>;
      order: number;
    }> = [];

    routes.optimizedSequences.forEach((sequence, seqIndex) => {
      sequence.routeSequence.forEach((route, routeIndex) => {
        if (route.originLocation && route.destinationLocation) {
          paths.push({
            path: [
              { lat: route.originLocation.lat, lng: route.originLocation.lng },
              {
                lat: route.destinationLocation.lat,
                lng: route.destinationLocation.lng,
              },
            ],
            order: seqIndex * 100 + routeIndex,
          });
        }
      });
    });

    return paths;
  }, [routes]);

  // Reset map to default view when routes are cleared
  useEffect(() => {
    if (!map) return;

    if (!routes) {
      map.setZoom(MAPS_CONFIG.defaultZoom);
      map.setCenter(MAPS_CONFIG.defaultCenter);
      return;
    }
  }, [map, routes]);

  // Fit map bounds to show all routes
  useEffect(() => {
    if (!map || !routes?.routes) return;

    const locations = getUniqueLocations();
    const allPoints: LatLng[] = [
      ...(locations.origin ? [locations.origin] : []),
      ...locations.destinations.map((d) => d.location),
    ];

    if (allPoints.length === 0) return;

    const bounds = new google.maps.LatLngBounds();
    allPoints.forEach((point) => {
      bounds.extend({ lat: point.lat, lng: point.lng });
    });

    map.fitBounds(bounds);

    const padding = { top: 100, right: 450, bottom: 50, left: 50 };
    map.fitBounds(bounds, padding);
  }, [map, routes, getUniqueLocations]);

  const locations = getUniqueLocations();
  const polylinePaths = getPolylinePaths();

  return (
    <GoogleMap
      key={
        routes ? `map-with-routes-${routes.totalDistanceMeters}` : "map-empty"
      }
      mapContainerStyle={MAPS_CONFIG.containerStyle}
      zoom={MAPS_CONFIG.defaultZoom}
      center={MAPS_CONFIG.defaultCenter}
      options={MAPS_CONFIG.options}
      onLoad={setMap}
      onUnmount={() => setMap(null)}
    >
      {locations.origin && (
        <Marker
          key="origin"
          position={{ lat: locations.origin.lat, lng: locations.origin.lng }}
          icon={svgMarker}
        />
      )}

      {locations.destinations.map(({ index, location, sequenceNumber }) => (
        <Marker
          key={`destination-${index}`}
          position={{ lat: location.lat, lng: location.lng }}
          label={{
            text: sequenceNumber.toString(),
            color: "#FFFFFF",
            fontWeight: "bold",
            fontSize: "14px",
          }}
        />
      ))}

      {routes &&
        polylinePaths.map((polyline, idx) => (
          <Polyline
            key={`polyline-${routes.totalDistanceMeters}-${idx}`}
            path={polyline.path}
            options={{
              strokeColor: getRouteColor(polyline.order),
              strokeOpacity: 0.8,
              strokeWeight: 4,
              icons: [
                {
                  icon: {
                    path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
                    scale: 3,
                    fillColor: getRouteColor(polyline.order),
                    fillOpacity: 1,
                    strokeColor: "#FFFFFF",
                    strokeWeight: 1,
                  },
                  offset: "100%",
                },
              ],
            }}
          />
        ))}
    </GoogleMap>
  );
};
