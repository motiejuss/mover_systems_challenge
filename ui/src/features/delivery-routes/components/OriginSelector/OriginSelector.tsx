import React from "react";
import { Box, HStack, Text, Badge, IconButton } from "@chakra-ui/react";
import { FiMapPin, FiTrash2 } from "react-icons/fi";
import type { SelectedPlace } from "../../types";

interface OriginSelectorProps {
  origin: SelectedPlace | null;
  autocompleteRef: React.RefObject<HTMLDivElement | null>;
  onRemove: () => void;
}

export const OriginSelector: React.FC<OriginSelectorProps> = ({
  origin,
  autocompleteRef,
  onRemove,
}) => {
  return (
    <Box>
      <HStack mb={1} gap={1}>
        <FiMapPin color="green" size={14} />
        <Text fontWeight="semibold" fontSize="sm">
          Origin
        </Text>
        <Badge colorScheme="green" fontSize="2xs" px={1} py={0}>
          Required
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
      {origin && (
        <Box mt={1} p={2} bg="green.50" borderRadius="md" fontSize="xs">
          <HStack justify="space-between" mb={0.5}>
            <Text
              fontWeight="bold"
              color="green.700"
              fontSize="xs"
              lineClamp={1}
            >
              {origin.address}
            </Text>
            <IconButton
              aria-label="Remove origin"
              size="2xs"
              colorScheme="red"
              variant="ghost"
              onClick={onRemove}
            >
              <FiTrash2 size={12} />
            </IconButton>
          </HStack>
          <Text color="gray.600" fontSize="2xs" lineClamp={1}>
            Place ID: {origin.placeId}
          </Text>
        </Box>
      )}
    </Box>
  );
};
