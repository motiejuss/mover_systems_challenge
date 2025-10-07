import type {
  DeliveryRouteRequest,
  DeliveryRouteResponse,
} from "../../features/delivery-routes/types";
import { API_CONFIG } from "../config/api.config";

export const calculateDeliveryRoutes = async (
  request: DeliveryRouteRequest
): Promise<DeliveryRouteResponse> => {
  const response = await fetch(`${API_CONFIG.baseUrl}/delivery-routes`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    let errorMessage = "Failed to calculate routes";

    try {
      const contentType = response.headers.get("content-type");

      if (
        contentType?.includes("application/json") ||
        contentType?.includes("application/problem+json")
      ) {
        const errorData = await response.json();

        // Handle ASP.NET Core ProblemDetails format
        if (errorData.title || errorData.detail) {
          errorMessage = errorData.detail || errorData.title;
        }
        // Handle simple string error responses
        else if (typeof errorData === "string") {
          errorMessage = errorData;
        }
        // Handle other error formats
        else if (errorData.message) {
          errorMessage = errorData.message;
        }
        // Handle validation errors
        else if (errorData.errors) {
          const errors = Object.values(errorData.errors).flat();
          errorMessage = errors.join(", ");
        }
      } else {
        // Plain text error
        const errorText = await response.text();
        errorMessage =
          errorText || `HTTP ${response.status}: ${response.statusText}`;
      }
    } catch {
      // If parsing fails, use a generic error message
      errorMessage = `HTTP ${response.status}: ${response.statusText}`;
    }

    throw new Error(errorMessage);
  }

  return response.json();
};
