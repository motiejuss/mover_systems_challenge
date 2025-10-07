import React from "react";
import { Box, VStack, Text, HStack, Badge } from "@chakra-ui/react";
import {
  parseDurationToMinutes,
  formatDistanceShort,
} from "../../utils/route.utils";
import type { DeliveryRouteResponse } from "../../types";

interface RouteResultsProps {
  routes: DeliveryRouteResponse;
}

export const RouteResults: React.FC<RouteResultsProps> = ({ routes }) => {
  return (
    <Box
      position="absolute"
      top={4}
      right={4}
      bg="white"
      p={3}
      borderRadius="md"
      boxShadow="lg"
      maxW="350px"
      maxH="80vh"
      overflowY="auto"
    >
      <VStack align="stretch" gap={2}>
        <Text fontSize="md" fontWeight="bold">
          Route Results
        </Text>

        <Box
          p={2}
          bg="blue.50"
          borderRadius="md"
          border="1px"
          borderColor="blue.200"
        >
          <HStack justify="space-between" fontSize="sm">
            <Text fontWeight="semibold">Total Distance:</Text>
            <Text fontWeight="bold" color="blue.700">
              {routes.totalDistanceMeters
                ? formatDistanceShort(routes.totalDistanceMeters)
                : "N/A"}
            </Text>
          </HStack>
          <HStack justify="space-between" fontSize="sm" mt={1}>
            <Text fontWeight="semibold">Total Time:</Text>
            <Text fontWeight="bold" color="blue.700">
              {routes.totalDuration
                ? `${parseDurationToMinutes(routes.totalDuration)} min`
                : "N/A"}
            </Text>
          </HStack>
          <HStack justify="space-between" fontSize="sm" mt={1}>
            <Text fontWeight="semibold">Calculation Time:</Text>
            <Text fontWeight="bold" color="blue.700">
              {routes.calculationTimeMs
                ? `${routes.calculationTimeMs.toFixed(0)} ms`
                : "N/A"}
            </Text>
          </HStack>
        </Box>

        {routes.optimizedSequences && routes.optimizedSequences.length > 0 ? (
          <>
            {routes.optimizedSequences.map((sequence, seqIndex) => (
              <Box key={seqIndex}>
                <Text
                  fontSize="xs"
                  fontWeight="bold"
                  mt={seqIndex > 0 ? 2 : 1}
                  color="gray.600"
                >
                  Route ({sequence.destinationsVisited} stops):
                </Text>

                <Box
                  p={2}
                  bg="green.50"
                  borderRadius="sm"
                  border="1px"
                  borderColor="green.200"
                  mt={1}
                >
                  <Text fontSize="xs" lineClamp={2}>
                    <strong>Origin:</strong> {sequence.originAddress}
                  </Text>
                </Box>

                {sequence.routeSequence.map((route, routeIndex) => (
                  <Box
                    key={routeIndex}
                    p={2}
                    bg="gray.50"
                    borderRadius="sm"
                    border="1px"
                    borderColor="gray.200"
                    mt={1}
                  >
                    <HStack mb={1} justify="space-between">
                      <Badge colorScheme="purple" size="sm">
                        Stop {routeIndex + 1}
                      </Badge>
                      <HStack gap={1} fontSize="xs">
                        <Text color="gray.600">
                          {formatDistanceShort(route.distanceMeters)}
                        </Text>
                        <Text color="gray.500">•</Text>
                        <Text color="gray.600">
                          {parseDurationToMinutes(route.duration)} min
                        </Text>
                      </HStack>
                    </HStack>

                    <VStack align="stretch" gap={0.5} fontSize="xs">
                      <Text lineClamp={1}>
                        <strong>From:</strong> {route.originAddress}
                      </Text>
                      <Text lineClamp={1}>
                        <strong>To:</strong> {route.destinationAddress}
                      </Text>
                    </VStack>
                  </Box>
                ))}
              </Box>
            ))}
          </>
        ) : (
          <>
            <Text fontSize="xs" color="gray.600">
              Showing {routes.routes.length} routes:
            </Text>

            {routes.routes.map((route, index) => (
              <Box
                key={index}
                p={2}
                bg="gray.50"
                borderRadius="sm"
                border="1px"
                borderColor="gray.200"
              >
                <HStack mb={1} justify="space-between">
                  <Badge colorScheme="blue" size="sm">
                    Route {index + 1}
                  </Badge>
                  <HStack gap={1} fontSize="xs">
                    <Text color="gray.600">
                      {formatDistanceShort(route.distanceMeters)}
                    </Text>
                    <Text color="gray.500">•</Text>
                    <Text color="gray.600">
                      {parseDurationToMinutes(route.duration)} min
                    </Text>
                  </HStack>
                </HStack>

                <VStack align="stretch" gap={0.5} fontSize="xs">
                  <Text lineClamp={1}>
                    <strong>From:</strong> {route.originAddress}
                  </Text>
                  <Text lineClamp={1}>
                    <strong>To:</strong> {route.destinationAddress}
                  </Text>
                </VStack>
              </Box>
            ))}
          </>
        )}
      </VStack>
    </Box>
  );
};
