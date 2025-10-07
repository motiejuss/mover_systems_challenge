import React from "react";
import {
  Box,
  HStack,
  Text,
  VStack,
  createListCollection,
  SelectRoot,
  SelectTrigger,
  SelectValueText,
  SelectContent,
  SelectItem,
} from "@chakra-ui/react";
import { FiCpu } from "react-icons/fi";
import type { Algorithm } from "../../types";

interface AlgorithmSelectorProps {
  value: Algorithm;
  onChange: (algorithm: Algorithm) => void;
}

const algorithmOptions = createListCollection({
  items: [
    {
      value: "NearestNeighbor",
      label: "Nearest Neighbor",
      description: "Fast greedy algorithm",
    },
    {
      value: "AStar",
      label: "A* Algorithm",
      description: "Optimal path search",
    },
  ],
});

export const AlgorithmSelector: React.FC<AlgorithmSelectorProps> = ({
  value,
  onChange,
}) => {
  return (
    <Box>
      <HStack mb={1} gap={1}>
        <FiCpu size={14} />
        <Text fontWeight="semibold" fontSize="sm">
          Algorithm
        </Text>
      </HStack>
      <SelectRoot
        collection={algorithmOptions}
        value={[value]}
        onValueChange={(e) => onChange(e.value[0] as Algorithm)}
        size="sm"
      >
        <SelectTrigger>
          <SelectValueText placeholder="Select algorithm" />
        </SelectTrigger>
        <SelectContent>
          {algorithmOptions.items.map((option) => (
            <SelectItem key={option.value} item={option}>
              <VStack align="start" gap={0}>
                <Text fontWeight="medium" fontSize="sm">
                  {option.label}
                </Text>
                <Text fontSize="2xs" color="gray.600">
                  {option.description}
                </Text>
              </VStack>
            </SelectItem>
          ))}
        </SelectContent>
      </SelectRoot>
    </Box>
  );
};
