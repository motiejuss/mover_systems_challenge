import React from "react";
import { Box, HStack, Text, Badge, IconButton, VStack } from "@chakra-ui/react";
import { FiMapPin, FiTrash2 } from "react-icons/fi";
import type { SelectedPlace } from "../../types";

interface DestinationListProps {
  destinations: SelectedPlace[];
  autocompleteRef: React.RefObject<HTMLDivElement | null>;
  onRemove: (index: number) => void;
}

export const DestinationList: React.FC<DestinationListProps> = ({
  destinations,
  autocompleteRef,
  onRemove,
}) => {
  return (
    <>
      <Box>
        <HStack mb={1} gap={1}>
          <FiMapPin color="red" size={14} />
          <Text fontWeight="semibold" fontSize="sm">
            Destinations
          </Text>
          <Badge colorScheme="red" fontSize="2xs" px={1} py={0}>
            {destinations.length}
          </Badge>
        </HStack>
        <Box
          ref={autocompleteRef}
          css={{
            "& gmp-basic-place-autocomplete": {
              width: "100%",
              height: "32px",
              borderRadius: "6px",
            },
          }}
        />
      </Box>

      {destinations.length > 0 && (
        <Box flex="1" display="flex" flexDirection="column" minH={0}>
          <Text fontWeight="semibold" mb={1} fontSize="xs" flexShrink={0}>
            Selected Destinations:
          </Text>
          <VStack
            gap={1.5}
            align="stretch"
            flex="1"
            overflowY="auto"
            p={1.5}
            bg="gray.50"
            borderRadius="md"
          >
            {destinations.map((dest, index) => (
              <Box
                key={dest.id + index}
                p={2}
                bg="white"
                borderRadius="md"
                border="1px"
                borderColor="gray.200"
              >
                <HStack justify="space-between" mb={0.5}>
                  <HStack gap={1.5}>
                    <Badge colorScheme="blue" fontSize="2xs" px={1} py={0}>
                      {index + 1}
                    </Badge>
                    <Text fontSize="xs" fontWeight="medium" lineClamp={1}>
                      {dest.address}
                    </Text>
                  </HStack>
                  <IconButton
                    aria-label="Remove destination"
                    size="2xs"
                    colorScheme="red"
                    variant="ghost"
                    onClick={() => onRemove(index)}
                  >
                    <FiTrash2 size={12} />
                  </IconButton>
                </HStack>
                <Text color="gray.600" fontSize="2xs" lineClamp={1}>
                  Place ID: {dest.placeId}
                </Text>
              </Box>
            ))}
          </VStack>
        </Box>
      )}
    </>
  );
};
