import React, { useState } from "react";
import { Box, Text, Button } from "@chakra-ui/react";
import { useLoadScript } from "@react-google-maps/api";
import { MAPS_CONFIG } from "../../../shared/config/maps.config";
import { RouteControlPanel } from "../components/RouteControlPanel";
import { RouteMap } from "../components/RouteMap";
import { RouteResults } from "../components/RouteResults";
import { useRouteCalculation } from "../hooks/useRouteCalculation";
import { usePlaceSelection } from "../hooks/usePlaceSelection";

declare global {
  interface Window {
    google: typeof google;
  }
}

export const DeliveryRoutesPage: React.FC = () => {
  const [showErrorAlert, setShowErrorAlert] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const {
    routeResponse,
    isCalculating,
    calculate,
    clear: clearRoutes,
  } = useRouteCalculation();
  const {
    origin,
    destinations,
    setOrigin,
    addDestination,
    removeDestination,
    clearAll: clearPlaces,
    canCalculate,
  } = usePlaceSelection();

  const clearAll = () => {
    clearPlaces();
    clearRoutes();
  };

  const handleError = (message: string) => {
    setErrorMessage(message);
    setShowErrorAlert(true);
  };

  const { isLoaded, loadError } = useLoadScript({
    googleMapsApiKey: MAPS_CONFIG.apiKey,
    libraries: MAPS_CONFIG.libraries,
  });

  if (loadError) {
    return (
      <Box display="flex" alignItems="center" justifyContent="center" h="100vh">
        <Text color="red.500">Error loading Google Maps</Text>
      </Box>
    );
  }

  if (!isLoaded) {
    return (
      <Box display="flex" alignItems="center" justifyContent="center" h="100vh">
        <Text>Loading map...</Text>
      </Box>
    );
  }

  return (
    <>
      {showErrorAlert && (
        <Box
          position="fixed"
          top="20px"
          left="50%"
          transform="translateX(-50%)"
          zIndex="9999"
          maxW="md"
          borderRadius="md"
          bg="red.100"
          color="red.800"
          border="1px solid"
          borderColor="red.300"
          p={4}
          boxShadow="lg"
          display="flex"
          alignItems="center"
          justifyContent="space-between"
        >
          <Box flex="1">
            <Text fontWeight="semibold">Error</Text>
            <Text fontSize="sm">{errorMessage}</Text>
          </Box>
          <Button
            size="sm"
            variant="ghost"
            colorScheme="red"
            onClick={() => setShowErrorAlert(false)}
            ml={2}
          >
            Close
          </Button>
        </Box>
      )}

      <Box display="flex" h="100vh" w="100vw">
        <RouteControlPanel
          isLoaded={isLoaded}
          onCalculateRoutes={calculate}
          isCalculating={isCalculating}
          origin={origin}
          destinations={destinations}
          setOrigin={setOrigin}
          addDestination={addDestination}
          removeDestination={removeDestination}
          clearAll={clearAll}
          canCalculate={canCalculate}
          onError={handleError}
        />
        <Box w="70%" h="100%" position="relative">
          <RouteMap origin={origin} routes={routeResponse} />
          {routeResponse && <RouteResults routes={routeResponse} />}
        </Box>
      </Box>
    </>
  );
};
