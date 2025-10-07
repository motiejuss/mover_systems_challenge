import React, { useState } from "react";
import {
  Box,
  VStack,
  Button,
  HStack,
  Heading,
  Text,
  Separator,
  Link,
} from "@chakra-ui/react";
import { createToaster } from "@chakra-ui/react";
import { AlgorithmSelector } from "../AlgorithmSelector";
import { VehicleEmissionSelector } from "../VehicleEmissionSelector";
import { OriginSelector } from "../OriginSelector";
import { DestinationList } from "../DestinationList";
import { useGoogleMapsAutocomplete } from "../../hooks/useGoogleMapsAutocomplete";
import type {
  Algorithm,
  VehicleEmissionType,
  DeliveryRouteResponse,
  SelectedPlace,
} from "../../types";
import destinations15Data from "./Destinations15.json";
import destinations25Data from "./Destinations25.json";
import destinationsError from "./DestinationsError.json";

interface RouteControlPanelProps {
  isLoaded: boolean;
  onCalculateRoutes: (
    origin: SelectedPlace,
    destinations: SelectedPlace[],
    algorithm: Algorithm,
    vehicleEmissionType: VehicleEmissionType
  ) => Promise<{
    success: boolean;
    data?: DeliveryRouteResponse;
    error?: string;
  }>;
  isCalculating: boolean;
  origin: SelectedPlace | null;
  destinations: SelectedPlace[];
  setOrigin: (origin: SelectedPlace | null) => void;
  addDestination: (destination: SelectedPlace) => void;
  removeDestination: (index: number) => void;
  clearAll: () => void;
  canCalculate: boolean;
  onError: (message: string) => void;
}

const toaster = createToaster({
  placement: "top",
  pauseOnPageIdle: true,
});

export const RouteControlPanel: React.FC<RouteControlPanelProps> = ({
  isLoaded,
  onCalculateRoutes,
  isCalculating,
  origin,
  destinations,
  setOrigin,
  addDestination,
  removeDestination,
  clearAll,
  canCalculate,
  onError,
}) => {
  const [algorithm, setAlgorithm] = useState<Algorithm>("NearestNeighbor");
  const [vehicleEmissionType, setVehicleEmissionType] =
    useState<VehicleEmissionType>("Electric");

  const originAutocompleteRef = useGoogleMapsAutocomplete({
    isLoaded,
    onPlaceSelect: setOrigin,
  });

  const destinationAutocompleteRef = useGoogleMapsAutocomplete({
    isLoaded,
    onPlaceSelect: addDestination,
  });

  const handleCalculateRoutes = async () => {
    if (!canCalculate || !origin) {
      toaster.create({
        title: "Missing Information",
        description: "Please select an origin and at least one destination",
        type: "warning",
        duration: 3000,
      });
      return;
    }

    const result = await onCalculateRoutes(
      origin,
      destinations,
      algorithm,
      vehicleEmissionType
    );

    if (result.success && result.data) {
      toaster.create({
        title: "Routes Calculated",
        description: `Successfully calculated ${result.data.routes.length} routes`,
        type: "success",
        duration: 3000,
      });
    } else {
      onError(result.error || "Failed to calculate routes");
    }
  };

  const handlePrepopulateDestinations = () => {
    destinations15Data.destinations.forEach((destination) => {
      addDestination(destination as SelectedPlace);
    });
  };

  const handlePrepopulate25Destinations = () => {
    destinations25Data.destinations.forEach((destination) => {
      addDestination(destination as SelectedPlace);
    });
  };

  const handlePrepopulateError = () => {
    destinationsError.destinations.forEach((destination) => {
      addDestination(destination as SelectedPlace);
    });
  };

  return (
    <Box
      w="30%"
      h="100%"
      bg="white"
      borderRight="1px"
      borderColor="gray.200"
      display="flex"
      flexDirection="column"
    >
      <VStack p={4} gap={3} align="stretch" flex="1" overflow="hidden">
        <Heading size="md">Delivery Routes</Heading>
        <Text color="gray.600" fontSize="xs">
          Select origin and destinations to calculate optimal routes or
          prepopulate destinations:{" "}
          <Link color={"blue"} onClick={handlePrepopulateDestinations}>
            15 in Sjælland
          </Link>
          {", "}
          <Link color={"blue"} onClick={handlePrepopulate25Destinations}>
            25 in Denmark
          </Link>
          {" or "}
          <Link color={"blue"} onClick={handlePrepopulateError}>
            provoke an error
          </Link>
        </Text>

        <OriginSelector
          origin={origin}
          autocompleteRef={originAutocompleteRef}
          onRemove={() => setOrigin(null)}
        />

        <Separator flexShrink={0} />

        <AlgorithmSelector value={algorithm} onChange={setAlgorithm} />

        <Separator flexShrink={0} />

        <VehicleEmissionSelector
          value={vehicleEmissionType}
          onChange={setVehicleEmissionType}
        />

        <Separator flexShrink={0} />

        <DestinationList
          destinations={destinations}
          autocompleteRef={destinationAutocompleteRef}
          onRemove={removeDestination}
        />
      </VStack>

      <Box p={3} borderTop="1px" borderColor="gray.200">
        <HStack gap={2}>
          <Button
            colorScheme="blue"
            flex="1"
            size="sm"
            disabled={!canCalculate || isCalculating}
            loading={isCalculating}
            onClick={handleCalculateRoutes}
          >
            Calculate Routes
          </Button>
          <Button
            variant="outline"
            colorScheme="gray"
            flex="1"
            size="sm"
            onClick={clearAll}
            disabled={!origin && destinations.length === 0}
          >
            Clear All
          </Button>
        </HStack>
      </Box>
    </Box>
  );
};
