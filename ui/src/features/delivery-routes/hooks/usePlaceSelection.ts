import { useState } from "react";
import type { SelectedPlace } from "../types";

export const usePlaceSelection = () => {
  const [origin, setOrigin] = useState<SelectedPlace | null>(null);
  const [destinations, setDestinations] = useState<SelectedPlace[]>([]);

  const addDestination = (destination: SelectedPlace) => {
    setDestinations((prev) => [...prev, destination]);
  };

  const removeDestination = (index: number) => {
    setDestinations((prev) => prev.filter((_, i) => i !== index));
  };

  const clearAll = () => {
    setOrigin(null);
    setDestinations([]);
  };

  const canCalculate = origin !== null && destinations.length > 0;

  return {
    origin,
    destinations,
    setOrigin,
    addDestination,
    removeDestination,
    clearAll,
    canCalculate,
  };
};
