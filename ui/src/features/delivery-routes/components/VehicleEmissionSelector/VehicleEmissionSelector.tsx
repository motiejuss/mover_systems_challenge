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
import { BsFuelPump } from "react-icons/bs";
import type { VehicleEmissionType } from "../../types";

interface VehicleEmissionSelectorProps {
  value: VehicleEmissionType;
  onChange: (emissionType: VehicleEmissionType) => void;
}

const emissionTypeOptions = createListCollection({
  items: [
    {
      value: "Electric",
      label: "Electric",
      description: "Zero emissions",
    },
    {
      value: "Gasoline",
      label: "Gasoline",
      description: "Standard fuel",
    },
    {
      value: "Hybrid",
      label: "Hybrid",
      description: "Electric + Gasoline",
    },
    {
      value: "Diesel",
      label: "Diesel",
      description: "Diesel fuel",
    },
  ],
});

export const VehicleEmissionSelector: React.FC<
  VehicleEmissionSelectorProps
> = ({ value, onChange }) => {
  return (
    <Box>
      <HStack mb={1} gap={1}>
        <BsFuelPump size={14} />
        <Text fontWeight="semibold" fontSize="sm">
          Vehicle Emission Type
        </Text>
      </HStack>
      <SelectRoot
        collection={emissionTypeOptions}
        value={[value]}
        onValueChange={(e) => onChange(e.value[0] as VehicleEmissionType)}
        size="sm"
      >
        <SelectTrigger>
          <SelectValueText placeholder="Select emission type" />
        </SelectTrigger>
        <SelectContent>
          {emissionTypeOptions.items.map((option) => (
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
