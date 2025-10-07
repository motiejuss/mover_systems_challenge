import { useState } from "react";
import type {
  DeliveryRouteResponse,
  SelectedPlace,
  Algorithm,
  VehicleEmissionType,
} from "../types";
import { calculateDeliveryRoutes } from "../../../shared/services/api.service";

interface RouteCalculationState {
  routeResponse: DeliveryRouteResponse | null;
  isCalculating: boolean;
  error: string | null;
}

export const useRouteCalculation = () => {
  const [state, setState] = useState<RouteCalculationState>({
    routeResponse: null,
    isCalculating: false,
    error: null,
  });

  const calculate = async (
    origin: SelectedPlace,
    destinations: SelectedPlace[],
    algorithm: Algorithm,
    vehicleEmissionType: VehicleEmissionType
  ) => {
    setState((prev) => ({ ...prev, isCalculating: true, error: null }));

    try {
      const response = await calculateDeliveryRoutes({
        origins: [origin],
        destinations,
        algorithm,
        vehicleEmissionType,
      });

      setState({
        routeResponse: response,
        isCalculating: false,
        error: null,
      });

      return { success: true, data: response };
    } catch (error) {
      const errorMessage =
        error instanceof Error ? error.message : "Failed to calculate routes";

      setState({
        routeResponse: null,
        isCalculating: false,
        error: errorMessage,
      });

      return { success: false, error: errorMessage };
    }
  };

  const clear = () => {
    setState({
      routeResponse: null,
      isCalculating: false,
      error: null,
    });
  };

  return {
    routeResponse: state.routeResponse,
    isCalculating: state.isCalculating,
    error: state.error,
    calculate,
    clear,
  };
};
